 $apikey = Read-Host -Prompt 'Ключ Api нугета'

.\.paket\paket.bootstrapper.exe

$version = Get-Content .\package.version

foreach ($nugetPackage in Get-ChildItem -Path .\nuget -Filter "*$version.nupkg" ) {
    .\.paket\paket.exe push url https://barsgroup.myget.org/F/kaliningrad/api/v2/package file $nugetPackage.FullName apikey $apikey
}
