 $buildconfig = Read-Host -Prompt 'Конфигурация сборки D - Debug (По умолчанию), R - Release'
 $version = Read-Host -Prompt 'Версия пакетов (По умолчанию из файла package.version)'

 if ($buildconfig -eq "D" -or $buildconfig -eq ""){
    $buildconfig = "Debug"
 }
 elseif ($buildconfig -eq "R"){
    $buildconfig = "Release"
 }

 if ($version -eq ""){

    $existVersion = $false;
    $existsFile = Test-Path "package.version"
    
    if ($existsFile -eq $true){
        $version = Get-Content .\package.version
        if ($version -ne $null -and $version -ne ""){
            $regExp = [regex]"(\d*)\.(\d*)\.(\d*)\.(\d*)(-\w*)?"
            if ($version -match $regExp -eq $true){
                $newBuildVersion = 1 + $Matches[4]
                $version = $version -ireplace "$regExp", "`$1.`$2.`$3.$newBuildVersion`$5"
                $existVersion = $true
            }
        }
    }
    
    if ($existVersion -eq $false){
        $version = "0.0.0.1"
    }
 }

Write-Host "Текущая версия пакетов $version"

.\.paket\paket.bootstrapper.exe
.\.paket\paket.exe pack output .\nuget version $version buildconfig $buildconfig 

Set-Content .\package.version $version
