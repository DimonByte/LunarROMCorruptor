$runtimes = @(
    "win-x64",
    "win-x86"
)

$baseName = "HostDownloader"

foreach ($rid in $runtimes) {

    $outputDir = "./publish/$rid"
    
    # Appends the platform directly to the binary name
    $customName = "$baseName-$rid"

    dotnet publish `
        -c Release `
        -f net10.0 `
        -r $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -p:AssemblyName=$customName `
        -o $outputDir

    # Generate checksums for all files in the publish folder
    Get-ChildItem $outputDir -File | ForEach-Object {
        # Skip hashing files that are already checksums
        if ($_.Extension -eq ".sha256") { return }

        $hash = Get-FileHash $_.FullName -Algorithm SHA256

        # Names it 'HostDownloader-linux-x64.sha256' or similar
        $checksumFile = "$($_.FullName).sha256"

        "$($hash.Hash)  $($_.Name)" |
            Out-File $checksumFile -Encoding ascii
    }
}