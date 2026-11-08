//MIT License

//Copyright (c) 2026 DimonByte

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using LunarROMCorruptor.EngineControls;
using LunarROMCorruptor.Modules;
using LunarROMCorruptor.Modules.CorruptionInternals;
using LunarROMCorruptor.Modules.CorruptionInternals.Engines;
using LunarROMCorruptor.Modules.Helpers;
using LunarROMCorruptor.Properties;
using System.Media;

namespace LunarROMCorruptor
{
    /// <summary>
    /// ByteView: rendering, zoom/pan, drag targeting and the range/region overlays (part of the main form).
    /// </summary>
    public partial class MainCorruptionForm
    {
        private ByteViewImage? _byteViewInfo; //The image currently shown in ByteView and how its pixels map to file offsets
        private bool _byteViewDragging;
        private long _byteViewDragStart;
        private long _byteViewDragEnd;
        private int _byteViewRenderVersion;
        private List<ExclusionEngine.NamedRegion>? _byteViewRegions; //Regions mode: the named regions of the shown file
        //Excluded ranges overlay, see GetExclusionOverlay: which image pixels are excluded, and one overlay bitmap per picture box
        private bool[]? _exclusionCoverage;
        private ByteRange[]? _exclusionCoverageRanges;
        private ByteViewImage? _exclusionCoverageView;
        private readonly Dictionary<PictureBox, (Bitmap Image, bool[] Coverage, int Width, int Height)> _exclusionOverlays = [];
        private string? _byteViewRegionProfile; //Regions mode: the profile the regions were read with

        private void InitializeByteView()
        {
            //ByteView targeting: drag on the image to set the start/end byte
            ByteViewPictureBox.Cursor = Cursors.Cross;
            ByteViewPictureBox.MouseDown += ByteViewPictureBox_MouseDown;
            ByteViewPictureBox.MouseMove += ByteViewPictureBox_MouseMove;
            ByteViewPictureBox.MouseUp += ByteViewPictureBox_MouseUp;
            ByteViewPictureBox.Paint += ByteViewPictureBox_Paint;
            ByteViewPictureBox.Resize += (_, _) =>
            {
                if (_byteViewInfo != null) ClampByteViewPan(_byteViewInfo);
                ByteViewPictureBox.Invalidate();
            };
            //Zoom: scroll wheel, middle mouse drag to move, double-click to reset (ByteView tab only, not the small preview)
            ByteViewPictureBox.MouseWheel += ByteViewPictureBox_MouseWheel;
            ByteViewPictureBox.DoubleClick += (_, _) =>
            {
                ResetByteViewZoom();
                if (_byteViewInfo != null) ByteViewInfoLbl.Text = DescribeByteView(_byteViewInfo);
                ByteViewPictureBox.Invalidate();
            };
            ByteViewRegionProfileComboBox.Items.Add("Auto");
            foreach (string profile in ExclusionEngine.Profiles.Keys) ByteViewRegionProfileComboBox.Items.Add(profile);
            ByteViewRegionProfileComboBox.SelectedIndex = 0;
            ByteViewRegionProfileComboBox.SelectedIndexChanged += (_, _) => { if (CorruptionCore.IsLoaded && SelectedByteViewMode == ByteViewMode.Regions) UpdateByteView(); };
            //The small ByteView preview on the main tab shows the same start/end range overlay
            ByteViewCloneImage.Paint += ByteViewPictureBox_Paint;
            ByteViewCloneImage.Resize += (_, _) => ByteViewCloneImage.Invalidate();
            ByteViewModeComboBox.SelectedIndexChanged += (_, _) =>
            {
                ByteViewRegionProfileComboBox.Visible = SelectedByteViewMode == ByteViewMode.Regions;
                if (CorruptionCore.IsLoaded && _byteViewInfo != null) UpdateByteView();
            };
        }

        private void ByteViewHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => ShowGuide("docs/ByteView.md");

        private void ByteViewSaveImageBtn_Click(object sender, EventArgs e)
        {
            var view = _byteViewInfo;
            if (view == null) return;
            using SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "PNG Files (*.png)|*.png|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save Image";
            if (saveFileDialog.ShowDialog() != DialogResult.OK) return;
            try
            {
                using Bitmap imageToSave = new(view.Image);
                var format = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant() is ".jpg" or ".jpeg" ? System.Drawing.Imaging.ImageFormat.Jpeg : System.Drawing.Imaging.ImageFormat.Png;
                imageToSave.Save(saveFileDialog.FileName, format);
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"Unable to save the ByteView image: {ex.Message}", StatusSeverityType.Error, true);
            }
        }

        private void ByteViewRefreshBtn_Click(object sender, EventArgs e)
        {
            if (!CorruptionCore.IsLoaded)
            {
                TraceLogger.Log("Attempted to refresh ByteView but no file is loaded. Please load a file before attempting to use ByteView.", StatusSeverityType.Error, true);
                return;
            }
            UpdateByteView();
        }

        private void UpdateByteViewIfEnabled()
        {
            if (ByteViewupdateWhenCorruptedChkBox.Checked)
            {
                UpdateByteView();
            }
        }

        private ByteViewMode SelectedByteViewMode => ByteViewModeComboBox.SelectedIndex switch
        {
            1 => ByteViewMode.Diff,
            2 => ByteViewMode.Entropy,
            3 => ByteViewMode.Regions,
            _ => ByteViewMode.Normal
        };

        /// <summary>
        /// Renders the latest corrupted file (or the loaded file if nothing has been corrupted yet) in ByteView on a background thread.
        /// In Regions mode the named regions of the file are read at the same time.
        /// </summary>
        private async void UpdateByteView()
        {
            string? path = CorruptionCore.LastOutputPath != null && File.Exists(CorruptionCore.LastOutputPath) ? CorruptionCore.LastOutputPath : CorruptionCore.SourcePath;
            if (path == null) return;
            bool colour = byteViewColourChkbox.Checked, flipH = flipHorizontalChkbox.Checked, flipV = flipVerticalChkbox.Checked;
            var mode = SelectedByteViewMode;
            string? original = CorruptionCore.SourcePath;
            string profileChoice = ByteViewRegionProfileComboBox.Text;
            //For in-place corruption the original isn't on disk anymore, so the Diff uses the stash instead
            long[]? stashOffsets = mode == ByteViewMode.Diff && CorruptionCore.PathsEqual(path, original) && CorruptionCore.Stash.Count > 0 ? [.. CorruptionCore.Stash.ByteOffsets] : null;
            int version = ++_byteViewRenderVersion;
            ByteViewRefreshBtn.Enabled = false;
            ByteViewInfoLbl.Text = "Rendering...";
            try
            {
                var (rendered, regions, profile) = await Task.Run(() =>
                {
                    var image = ByteView.Render(path, mode, colour, flipH, flipV, original, stashOffsets);
                    List<ExclusionEngine.NamedRegion>? list = null;
                    string? detected = null;
                    if (image != null && mode == ByteViewMode.Regions)
                    {
                        using var data = FileBuffer.OpenRead(path);
                        detected = string.IsNullOrEmpty(profileChoice) || profileChoice == "Auto" ? ExclusionEngine.DetectProfile(original ?? path, data) : profileChoice;
                        list = detected != null ? ExclusionEngine.GetNamedRegions(detected, data) : [];
                        //Big areas first, so the small ones are drawn on top of them
                        list = [.. list.OrderByDescending(r => r.End - r.Start)];
                    }
                    return (image, list, detected);
                });
                if (version != _byteViewRenderVersion || rendered == null)
                {
                    rendered?.Dispose(); //A newer render was started (or this one failed)
                    if (rendered == null) TraceLogger.Log("ByteView conversion failed: no image produced.", StatusSeverityType.Error);
                    return;
                }
                var old = _byteViewInfo;
                if (old == null || old.Width != rendered.Width || old.Height != rendered.Height || old.FlipH != rendered.FlipH || old.FlipV != rendered.FlipV)
                    ResetByteViewZoom(); //Keep the zoom when the same layout is re-rendered (e.g. after a corruption)
                _byteViewInfo = rendered;
                _byteViewRegions = regions;
                _byteViewRegionProfile = profile;
                ByteViewCloneImage.Image = rendered.Image;
                old?.Dispose();
                ByteViewInfoLbl.Text = DescribeByteView(rendered);
                InvalidateByteViewOverlays();
            }
            catch (Exception ex)
            {
                TraceLogger.Log($"ByteView failed: {ex.Message}", StatusSeverityType.Error);
                ByteViewInfoLbl.Text = "ByteView failed, see the log.";
            }
            finally
            {
                ByteViewRefreshBtn.Enabled = !_busy;
            }
        }

        private string DescribeByteView(ByteViewImage view)
        {
            string scale = $"1 px = {view.BytesPerPixel:N0} bytes";
            if (_byteViewZoom > 1f) scale += $", zoom {_byteViewZoom:0.#}x";
            return view.Mode switch
            {
                ByteViewMode.Diff when view.ChangedBytes >= 0 => $"{view.ChangedBytes:N0} changed bytes (red). {scale}",
                ByteViewMode.Entropy => $"Dark = empty, blue/green = text/tables, yellow = code, red = compressed. {scale}",
                ByteViewMode.Regions => _byteViewRegionProfile == null
                    ? $"Unknown file type: pick a profile below. {scale}"
                    : $"{_byteViewRegionProfile}: {(_byteViewRegions?.Count ?? 0)} region(s). Hover for names. {scale}",
                _ => $"Drag to set the byte range, right-click to reset. {scale}"
            };
        }

        private void ClearByteView()
        {
            _byteViewRenderVersion++;
            ByteViewPictureBox.Image = null;
            ByteViewCloneImage.Image = null; //Shares the bitmap that is disposed below
            _byteViewInfo?.Dispose();
            _byteViewInfo = null;
            _byteViewRegions = null;
            DisposeExclusionOverlay();
            _byteViewRegionProfile = null;
            ResetByteViewZoom();
            ByteViewPictureBox.Invalidate();
            ByteViewInfoLbl.Text = "Drag on the image to set the byte range.";
        }

        /// <summary>
        /// Redraws the overlays (range, regions, excluded ranges, bisect) on the ByteView tab and on the small preview on the main tab.
        /// </summary>
        internal void InvalidateByteViewOverlays()
        {
            ByteViewPictureBox.Invalidate();
            ByteViewCloneImage.Invalidate();
        }

        #region ByteView zoom

        private const float MaxByteViewZoom = 64f;
        private float _byteViewZoom = 1f; //1 = the whole image fits in the picture box
        private PointF _byteViewPan; //Offset of the zoomed image from its centred position
        private bool _byteViewPanning;
        private Point _byteViewPanStart;
        private PointF _byteViewPanOrigin;

        private void ResetByteViewZoom()
        {
            _byteViewZoom = 1f;
            _byteViewPan = PointF.Empty;
            _byteViewPanning = false;
        }

        /// <summary>
        /// Keeps the zoomed image covering the picture box (no panning past its edges).
        /// </summary>
        private void ClampByteViewPan(ByteViewImage view)
        {
            var client = ByteViewPictureBox.ClientSize;
            float scale = Math.Min((float)client.Width / view.Width, (float)client.Height / view.Height) * _byteViewZoom;
            float halfX = Math.Max(0, (view.Width * scale - client.Width) / 2f), halfY = Math.Max(0, (view.Height * scale - client.Height) / 2f);
            _byteViewPan = new PointF(Math.Clamp(_byteViewPan.X, -halfX, halfX), Math.Clamp(_byteViewPan.Y, -halfY, halfY));
        }

        private void ByteViewPictureBox_MouseWheel(object? sender, MouseEventArgs e)
        {
            var view = _byteViewInfo;
            if (view == null) return;
            var before = GetByteViewImageRectangle(view, ByteViewPictureBox);
            float ix = Math.Clamp((e.X - before.X) / before.Width, 0f, 1f), iy = Math.Clamp((e.Y - before.Y) / before.Height, 0f, 1f);
            float zoom = Math.Clamp(_byteViewZoom * (e.Delta > 0 ? 1.25f : 0.8f), 1f, MaxByteViewZoom);
            if (Math.Abs(zoom - _byteViewZoom) < 0.001f) return;
            _byteViewZoom = zoom < 1.01f ? 1f : zoom;
            _byteViewPan = PointF.Empty;
            if (_byteViewZoom > 1f)
            {
                //Keep the point under the mouse in the same place
                var centred = GetByteViewImageRectangle(view, ByteViewPictureBox);
                _byteViewPan = new PointF(e.X - ix * centred.Width - centred.X, e.Y - iy * centred.Height - centred.Y);
                ClampByteViewPan(view);
            }
            ByteViewInfoLbl.Text = DescribeByteView(view);
            ByteViewPictureBox.Invalidate();
        }

        #endregion

        /// <summary>
        /// Where the image is drawn inside a picture box: fitted and centred (like SizeMode = Zoom), plus the scroll wheel zoom on the ByteView tab.
        /// </summary>
        private RectangleF GetByteViewImageRectangle(ByteViewImage view) => GetByteViewImageRectangle(view, ByteViewPictureBox);

        private RectangleF GetByteViewImageRectangle(ByteViewImage view, PictureBox box)
        {
            var client = box.ClientSize;
            bool zoomed = box == ByteViewPictureBox && _byteViewZoom > 1f;
            float scale = Math.Min((float)client.Width / view.Width, (float)client.Height / view.Height) * (zoomed ? _byteViewZoom : 1f);
            float w = view.Width * scale, h = view.Height * scale;
            float x = (client.Width - w) / 2f, y = (client.Height - h) / 2f;
            if (zoomed)
            {
                x += _byteViewPan.X;
                y += _byteViewPan.Y;
            }
            return new RectangleF(x, y, w, h);
        }

        /// <summary>
        /// Converts a mouse position on the picture box to a file offset. Returns -1 outside of the image.
        /// </summary>
        private long ByteViewOffsetAt(Point location, bool clampToImage)
        {
            var view = _byteViewInfo;
            if (view == null) return -1;
            var rect = GetByteViewImageRectangle(view);
            if (!clampToImage && !rect.Contains(location)) return -1;
            int x = (int)Math.Clamp((location.X - rect.X) * view.Width / rect.Width, 0, view.Width - 1);
            int y = (int)Math.Clamp((location.Y - rect.Y) * view.Height / rect.Height, 0, view.Height - 1);
            long offset = view.OffsetOf(x, y);
            if (offset < 0 && clampToImage) offset = view.FileLength - 1; //Past the end of the data on the last row
            return offset;
        }

        private void ByteViewPictureBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (_byteViewInfo == null) return;
            if (e.Button == MouseButtons.Middle)
            {
                if (_byteViewZoom > 1f)
                {
                    _byteViewPanning = true;
                    _byteViewPanStart = e.Location;
                    _byteViewPanOrigin = _byteViewPan;
                    ByteViewPictureBox.Cursor = Cursors.SizeAll;
                }
                return;
            }
            if (!CorruptionCore.IsLoaded || CorruptionQueueChkbox.Checked || _busy) return;
            if (e.Button == MouseButtons.Right)
            {
                //Right-click resets the range to the whole file
                StartByteNumb.Value = 0;
                EndByteNumb.Value = EndByteNumb.Maximum;
                TraceLogger.Log("ByteView: byte range reset to the whole file.");
                return;
            }
            if (e.Button != MouseButtons.Left) return;
            long offset = ByteViewOffsetAt(e.Location, false);
            if (offset < 0) return;
            _byteViewDragging = true;
            _byteViewDragStart = _byteViewDragEnd = offset;
            InvalidateByteViewOverlays();
        }

        /// <summary>
        /// The smallest named region that contains an offset (Regions mode hover).
        /// </summary>
        private ExclusionEngine.NamedRegion? RegionAt(long offset)
        {
            var regions = _byteViewRegions;
            if (regions == null) return null;
            ExclusionEngine.NamedRegion? best = null;
            foreach (var r in regions)
            {
                if (offset < r.Start || offset > r.End) continue;
                if (best == null || r.End - r.Start < best.Value.End - best.Value.Start) best = r;
            }
            return best;
        }

        private void ByteViewPictureBox_MouseMove(object? sender, MouseEventArgs e)
        {
            var view = _byteViewInfo;
            if (view == null) return;
            if (_byteViewPanning)
            {
                _byteViewPan = new PointF(_byteViewPanOrigin.X + e.X - _byteViewPanStart.X, _byteViewPanOrigin.Y + e.Y - _byteViewPanStart.Y);
                ClampByteViewPan(view);
                ByteViewPictureBox.Invalidate();
                return;
            }
            if (_byteViewDragging)
            {
                _byteViewDragEnd = ByteViewOffsetAt(e.Location, true);
                long a = Math.Min(_byteViewDragStart, _byteViewDragEnd), b = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1;
                ByteViewInfoLbl.Text = $"0x{a:X} - 0x{Math.Min(b, view.FileLength - 1):X}\n({FormatByteCount(Math.Min(b, view.FileLength - 1) - a + 1)})";
                ByteViewPictureBox.Invalidate();
                return;
            }
            long offset = ByteViewOffsetAt(e.Location, false);
            if (offset < 0)
            {
                ByteViewInfoLbl.Text = DescribeByteView(view);
                return;
            }
            string text = $"Offset 0x{offset:X}\n({offset:N0})";
            if (ExcludeList.Contains(CorruptionCore.Exclusions.Snapshot(), offset)) text += "\nExcluded (no engine changes it)";
            else if (view.Mode == ByteViewMode.Regions && RegionAt(offset) is { } region) text += $"\n{region.Name}";
            else text += $"\n1 px = {view.BytesPerPixel:N0} bytes";
            ByteViewInfoLbl.Text = text;
        }

        private void ByteViewPictureBox_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_byteViewPanning && e.Button == MouseButtons.Middle)
            {
                _byteViewPanning = false;
                ByteViewPictureBox.Cursor = Cursors.Cross;
                return;
            }
            if (!_byteViewDragging || _byteViewInfo == null) return;
            _byteViewDragging = false;
            var view = _byteViewInfo;
            long start = Math.Min(_byteViewDragStart, _byteViewDragEnd);
            long end = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1; //Include every byte of the last pixel
            if (_byteViewDragStart == _byteViewDragEnd)
            {
                //A single click just shows the offset, it doesn't change the range
                InvalidateByteViewOverlays();
                return;
            }
            start = Math.Clamp(start, 0, (long)EndByteNumb.Maximum);
            end = Math.Clamp(end, start, (long)EndByteNumb.Maximum);
            //Set the end first if the new start is past the current end, so start <= end is kept
            if (start > EndByteNumb.Value)
            {
                EndByteNumb.Value = end;
                StartByteNumb.Value = start;
            }
            else
            {
                StartByteNumb.Value = start;
                EndByteNumb.Value = end;
            }
            TraceLogger.Log($"ByteView: byte range set to 0x{start:X} - 0x{end:X} ({FormatByteCount(end - start + 1)}).");
            ByteViewInfoLbl.Text = $"Range set:\n0x{start:X} - 0x{end:X}\n({FormatByteCount(end - start + 1)})";
            InvalidateByteViewOverlays();
        }


        /// <summary>
        /// The image rectangles (in image pixels, before flips) that cover the bytes a to b.
        /// </summary>
        private static List<Rectangle> RangeRects(ByteViewImage view, long a, long b)
        {
            long pixels = (long)view.Width * view.Height;
            long pa = Math.Clamp(a / view.BytesPerPixel, 0, pixels - 1);
            long pb = Math.Clamp(b / view.BytesPerPixel, pa, pixels - 1);
            long ra = pa / view.Width, rb = pb / view.Width;
            var rects = new List<Rectangle>(3);
            if (ra == rb)
            {
                rects.Add(new Rectangle((int)(pa % view.Width), (int)ra, (int)(pb - pa + 1), 1));
            }
            else
            {
                rects.Add(new Rectangle((int)(pa % view.Width), (int)ra, (int)(view.Width - pa % view.Width), 1));
                if (rb - ra > 1) rects.Add(new Rectangle(0, (int)ra + 1, view.Width, (int)(rb - ra - 1)));
                rects.Add(new Rectangle(0, (int)rb, (int)(pb % view.Width + 1), 1));
            }
            return rects;
        }

        /// <summary>
        /// Draws part of an image at the ByteView scale. Sharp uses nearest neighbour, so zoomed-in pixels stay square.
        /// </summary>
        private static void DrawImagePart(Graphics g, Image image, RectangleF dest, RectangleF source, bool sharp)
        {
            if (sharp)
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            }
            g.DrawImage(image, dest, source, GraphicsUnit.Pixel);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
        }

        /// <summary>
        /// The excluded ranges as a transparent overlay for one picture box, or null when nothing is excluded. Everything is cached:
        /// the exclusion list returns the same array until it changes, so comparing references is enough.
        /// When the image is shown at least 1:1 the overlay has one pixel per image pixel (drawn like the image, sharp and cropped).
        /// When it's shrunk, the overlay is made at the shown size instead, and a screen pixel is red if ANY image pixel under it is
        /// excluded. Shrinking a full-size overlay would blend single excluded pixels away, so small ranges (like a Bisect suspect)
        /// would disappear.
        /// </summary>
        private (Bitmap Image, bool FullSize)? GetExclusionOverlay(ByteViewImage view, ByteRange[] ranges, PictureBox box, RectangleF area)
        {
            if (ranges.Length == 0)
            {
                DisposeExclusionOverlay();
                return null;
            }
            try
            {
                if (_exclusionCoverage == null || !ReferenceEquals(ranges, _exclusionCoverageRanges) || !ReferenceEquals(view, _exclusionCoverageView))
                {
                    DisposeExclusionOverlay();
                    _exclusionCoverage = RangeCoverage(view, ranges);
                    _exclusionCoverageRanges = ranges;
                    _exclusionCoverageView = view;
                }
                bool fullSize = area.Width >= view.Width && area.Height >= view.Height;
                int w = fullSize ? view.Width : Math.Clamp((int)Math.Round(area.Width), 1, view.Width);
                int h = fullSize ? view.Height : Math.Clamp((int)Math.Round(area.Height), 1, view.Height);
                if (_exclusionOverlays.TryGetValue(box, out var cached))
                {
                    if (ReferenceEquals(cached.Coverage, _exclusionCoverage) && cached.Width == w && cached.Height == h)
                        return (cached.Image, fullSize);
                    cached.Image.Dispose();
                    _exclusionOverlays.Remove(box);
                }
                var grid = ScreenGrid(view, _exclusionCoverage, w, h);
                var image = OverlayBitmap(OverlayPixels(grid, w, h, Premultiply(RegionColour(ExclusionEngine.RegionKind.Excluded)), Premultiply(Color.FromArgb(200, 255, 60, 255))), w, h);
                _exclusionOverlays[box] = (image, _exclusionCoverage, w, h);
                return (image, fullSize);
            }
            catch (Exception ex) when (ex is ArgumentException or OutOfMemoryException or OverflowException)
            {
                TraceLogger.Log($"ByteView: couldn't draw the excluded ranges: {ex.Message}", StatusSeverityType.Warning);
                return null;
            }

            static int Premultiply(Color c) => (c.A << 24) | ((c.R * c.A / 255) << 16) | ((c.G * c.A / 255) << 8) | (c.B * c.A / 255);
        }

        private void DisposeExclusionOverlay()
        {
            foreach (var overlay in _exclusionOverlays.Values) overlay.Image.Dispose();
            _exclusionOverlays.Clear();
            _exclusionCoverage = null;
            _exclusionCoverageRanges = null;
            _exclusionCoverageView = null;
        }

        /// <summary>
        /// Which image pixels (in image order, before flips) contain at least one byte of the sorted ranges.
        /// Each pixel is written once, so this is O(ranges + pixels).
        /// </summary>
        internal static bool[] RangeCoverage(ByteViewImage view, ByteRange[] sorted)
        {
            int count = checked(view.Width * view.Height);
            long lastByte = view.FileLength - 1;
            var covered = new bool[count];
            long next = 0; //First pixel not filled yet. The ranges are sorted, so the spans only move forward.
            foreach (var r in sorted)
            {
                if (r.Start > lastByte) break;
                if (r.End < 0) continue;
                long pa = Math.Clamp(Math.Max(0, r.Start) / view.BytesPerPixel, 0, count - 1);
                long pb = Math.Clamp(Math.Min(r.End, lastByte) / view.BytesPerPixel, pa, count - 1);
                if (pa < next) pa = next;
                if (pa > pb) continue; //Already covered (many ranges inside one pixel)
                Array.Fill(covered, true, (int)pa, (int)(pb - pa + 1));
                next = pb + 1;
            }
            return covered;
        }

        /// <summary>
        /// The coverage in screen order (flips applied) at w x h. Smaller than the image: a cell is set if any image pixel in it is.
        /// </summary>
        internal static bool[] ScreenGrid(ByteViewImage view, bool[] covered, int w, int h)
        {
            int iw = view.Width, ih = view.Height;
            var grid = new bool[w * h];
            for (int iy = 0; iy < ih; iy++)
            {
                int y = view.FlipV ? ih - 1 - iy : iy;
                int gy = (int)((long)y * h / ih);
                int rowStart = iy * iw;
                for (int ix = 0; ix < iw; ix++)
                {
                    if (!covered[rowStart + ix]) continue;
                    int x = view.FlipH ? iw - 1 - ix : ix;
                    grid[gy * w + (int)((long)x * w / iw)] = true;
                }
            }
            return grid;
        }

        /// <summary>
        /// Overlay pixels for a grid: set cells get the fill colour, set cells next to an unset one (or the border) get the edge colour.
        /// </summary>
        internal static int[] OverlayPixels(bool[] grid, int w, int h, int fillArgb, int edgeArgb)
        {
            var pixels = new int[w * h];
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    int i = row + x;
                    if (!grid[i]) continue;
                    bool isEdge = x == 0 || x == w - 1 || y == 0 || y == h - 1 || !grid[i - 1] || !grid[i + 1] || !grid[i - w] || !grid[i + w];
                    pixels[i] = isEdge ? edgeArgb : fillArgb;
                }
            }
            return pixels;
        }

        private static Bitmap OverlayBitmap(int[] pixels, int w, int h)
        {
            var bitmap = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); //Premultiplied ARGB draws fastest
            var data = bitmap.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            try
            {
                for (int y = 0; y < h; y++)
                    System.Runtime.InteropServices.Marshal.Copy(pixels, y * w, data.Scan0 + y * data.Stride, w);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
            return bitmap;
        }

        private static void DrawRange(Graphics g, ByteViewImage view, RectangleF area, long a, long b, Brush fill, Pen? outline)
        {
            if (b < 0 || a >= view.FileLength) return;
            float sx = area.Width / view.Width, sy = area.Height / view.Height;
            foreach (var r in RangeRects(view, Math.Max(0, a), Math.Min(b, view.FileLength - 1)))
            {
                int x = view.FlipH ? view.Width - (r.X + r.Width) : r.X;
                int y = view.FlipV ? view.Height - (r.Y + r.Height) : r.Y;
                var rf = new RectangleF(area.X + x * sx, area.Y + y * sy, Math.Max(1f, r.Width * sx), Math.Max(1f, r.Height * sy));
                g.FillRectangle(fill, rf);
                if (outline != null) g.DrawRectangle(outline, rf.X, rf.Y, rf.Width, rf.Height);
            }
        }

        private static Color RegionColour(ExclusionEngine.RegionKind kind) => kind switch
        {
            ExclusionEngine.RegionKind.Protected => Color.FromArgb(130, 255, 140, 0),
            ExclusionEngine.RegionKind.Code => Color.FromArgb(70, 60, 140, 255),
            ExclusionEngine.RegionKind.Graphics => Color.FromArgb(85, 255, 90, 200),
            ExclusionEngine.RegionKind.Compressed => Color.FromArgb(110, 170, 80, 255),
            ExclusionEngine.RegionKind.Excluded => Color.FromArgb(140, 255, 30, 30),
            _ => Color.FromArgb(45, 60, 220, 120)
        };

        /// <summary>
        /// Draws the ByteView image (ByteView tab only, with zoom) and the overlays: regions (Regions mode), excluded ranges (red),
        /// the half left out of the current bisect test (cyan) and the selected/current byte range (yellow).
        /// </summary>
        private void ByteViewPictureBox_Paint(object? sender, PaintEventArgs e)
        {
            var view = _byteViewInfo;
            var box = sender as PictureBox ?? ByteViewPictureBox; //Used by both the ByteView tab and the small preview (ByteViewCloneImage)
            if (view == null) return;
            var g = e.Graphics;
            var area = GetByteViewImageRectangle(view, box);
            //The part of the image that is on screen (source pixels and where they go). The small preview always shows all of it.
            var source = new RectangleF(0, 0, view.Width, view.Height);
            var dest = area;
            bool sharp = false;
            if (box == ByteViewPictureBox)
            {
                //Only the visible part of the image is drawn, with sharp pixels once a pixel is bigger than a screen pixel
                float sx = area.Width / view.Width, sy = area.Height / view.Height;
                var client = box.ClientRectangle;
                int x0 = (int)Math.Clamp(Math.Floor((client.Left - area.X) / sx), 0, view.Width - 1);
                int y0 = (int)Math.Clamp(Math.Floor((client.Top - area.Y) / sy), 0, view.Height - 1);
                int x1 = (int)Math.Clamp(Math.Ceiling((client.Right - area.X) / sx), x0 + 1, view.Width);
                int y1 = (int)Math.Clamp(Math.Ceiling((client.Bottom - area.Y) / sy), y0 + 1, view.Height);
                source = new RectangleF(x0, y0, x1 - x0, y1 - y0);
                dest = new RectangleF(area.X + x0 * sx, area.Y + y0 * sy, (x1 - x0) * sx, (y1 - y0) * sy);
                sharp = sx >= 1f;
                DrawImagePart(g, view.Image, dest, source, sharp);
            }
            else if (box.Image == null) return;

            //Regions
            var regions = _byteViewRegions;
            if (view.Mode == ByteViewMode.Regions && regions != null)
            {
                foreach (var r in regions)
                {
                    using var fill = new SolidBrush(RegionColour(r.Kind));
                    DrawRange(g, view, area, r.Start, r.End, fill, null);
                }
            }

            //Excluded ranges are always shown, in red with a magenta edge. They are drawn once into an overlay bitmap (rebuilt only when
            //the ranges or the image change), so a paint costs one DrawImage however many ranges there are.
            if (GetExclusionOverlay(view, CorruptionCore.Exclusions.Snapshot(), box, area) is { } overlay)
            {
                if (overlay.FullSize)
                    DrawImagePart(g, overlay.Image, dest, source, sharp);
                else //Made at the shown size: drawn 1:1 over the whole image (anything off screen is clipped)
                    DrawImagePart(g, overlay.Image, area, new RectangleF(0, 0, overlay.Image.Width, overlay.Image.Height), true);
            }

            //Bisect: the half that the current test leaves out
            if (_bisectForm is { Visible: true, Session: { TestPending: true, RemovedSpan: { } removed } })
            {
                using var fill = new SolidBrush(Color.FromArgb(70, 0, 220, 255));
                using var outline = new Pen(Color.FromArgb(220, 0, 220, 255));
                DrawRange(g, view, area, removed.Start, removed.End, fill, outline);
            }

            //Selected byte range
            long a, b;
            if (_byteViewDragging && box == ByteViewPictureBox)
            {
                a = Math.Min(_byteViewDragStart, _byteViewDragEnd);
                b = Math.Max(_byteViewDragStart, _byteViewDragEnd) + view.BytesPerPixel - 1;
            }
            else
            {
                if (!CorruptionCore.IsLoaded || CorruptionQueueChkbox.Checked) return;
                a = (long)StartByteNumb.Value;
                b = (long)EndByteNumb.Value;
                if (a == 0 && b >= view.FileLength - 1) return; //Whole file selected, nothing to highlight
            }
            using (var fill = new SolidBrush(Color.FromArgb(_byteViewDragging ? 110 : 70, 255, 230, 0)))
            using (var outline = new Pen(Color.FromArgb(220, 255, 230, 0)))
                DrawRange(g, view, area, a, b, fill, outline);
        }

        private void ByteViewColourChkbox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewColourMode = byteViewColourChkbox.Checked;
            Settings.Default.Save();
        }

        private void ByteViewupdateWhenCorruptedChkBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ByteViewUpdateOnCorruption = ByteViewupdateWhenCorruptedChkBox.Checked;
            Settings.Default.Save();
        }

        private void ByteViewCloneImage_Click(object sender, EventArgs e)
        {
            MainTabControl.SelectedTab = ByteViewTab;
        }
    }
}
