  
param (
    $buildName ="Pawnmorpher",
    $buildDir = "Build" ,
    $VSVersion = "2019",
    $OutVersion = ""
)


If(!(test-path $buildDir))
{
      New-Item -ItemType Directory -Force -Path $buildDir
}

If(!(test-path "$buildDir/Tmp"))
{
    New-Item -ItemType Directory -Force -Path "$buildDir\Tmp"

}else { 
    Remove-Item -Recurse -Force "$buildDir\Tmp"
    New-Item -ItemType Directory -Force -Path "$buildDir\Tmp" 
    #clean contents of temp directory 
}

If(Test-Path "$buildDir/$buildName$OutVersion.zip")
{
    Remove-Item -Path "$buildDir/$buildName$OutVersion.zip" -Force
}

dotnet restore "Source/Pawnmorphs/Pawnmorph.sln"

if(!$?)
{
    Write-Error "could not restore project"
    exit 1 
}

."C:\Program Files (x86)\Microsoft Visual Studio\$VSVersion\Community\MSBuild\Current\Bin\MSBuild.exe"  "Source/Pawnmorphs/Pawnmorph.sln" /t:restore


if(!$?)
{
    Write-Error "could not restore project"
    exit 1 
}


."C:\Program Files (x86)\Microsoft Visual Studio\$VSVersion\Community\MSBuild\Current\Bin\MSBuild.exe"  "Source/Pawnmorphs/Pawnmorph.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="any cpu"

if(!$?)
{
    Write-Error "could not build project"
    exit 1 
}

Copy-Item -Path Defs, About, "1.2", "1.1", "1.0" , Languages, Patches, Textures -Destination "$buildDir/Tmp" -Recurse
Copy-Item -Path LoadFolders.xml -Destination "$buildDir/Tmp/LoadFolders.xml" 

#Remove hugs lib dll if present 
if(Test-Path "$buildDir/Tmp/1.1/Assemblies/HugsLib.dll")
{
    Remove-Item "$buildDir/Tmp/1.1/Assemblies/HugsLib.dll" 
}


#check for .vs folders and get rid of them  
if(Test-Path "$buildDir/Tmp/.vs")
{
    Remove-Item "$buildDir/Tmp/.vs" -Recurse -Force 
}

if(Test-Path "$buildDir/Tmp/Source/Pawnmorphs/.vs")
{
    Remove-Item "$buildDir/Tmp/Source/Pawnmorphs/.vs" -Recurse -Force
}
#get rid of nuget packages

if(Test-Path "$buildDir/Tmp/Source/Pawnmorphs/packages")
{
    Remove-Item "$buildDir/Tmp/Source/Pawnmorphs/packages" -Force -Recurse
}

Compress-Archive -Path  "$buildDir/Tmp/*" -CompressionLevel Optimal -Force -DestinationPath "$buildDir/$buildName-$OutVersion $(get-date -f MM-dd).zip"

if(!$?)
{
    Write-Error "unable to create archive"
    exit 1
}

Remove-Item -Path "$buildDir/Tmp" -Recurse -Force 
Write-Output "file $buildDir/$buildName-$OutVersion $(get-date -f MM-dd).zip created successfully" 
