param (
    $buildName ="Pawnmorpher.zip",
    $buildDir = "Build" ,
    $VSVersion = "2019"
)


If(!(test-path $buildDir))
{
      New-Item -ItemType Directory -Force -Path $buildDir
}

If(!(test-path "$buildDir/Tmp"))
{
    New-Item -ItemType Directory -Force -Path "$buildDir\Tmp"
}

If(Test-Path "$buildDir/$buildName")
{
    Remove-Item -Path "$buildDir/$buildName" -Force
}

Write-Output "building solution"

If(!(. "C:\Program Files (x86)\Microsoft Visual Studio\$VSVersion\Community\MSBuild\Current\Bin\MSBuild.exe"  "Source/Pawnmorphs/Pawnmorph.sln" /t:Rebuild /p:Configuration=Debug /p:Platform="any cpu")) 
{
    exit 1 
}
Write-Output "build successfull" 

Copy-Item -Path Defs, About,Assemblies,Languages,Patches, Source,Textures -Destination "$buildDir/Tmp" -Recurse
if(test-path "$buildDir\Tmp\Source\Pawnmorphs\.vs")
{
    Write-Output "removing .vs folder"
    Remove-Item -Path "$buildDir\Tmp\Source\Pawnmorphs\.vs" -Recurse -Force
} 

Compress-Archive -Path "$buildDir/Tmp/*" -CompressionLevel Optimal -DestinationPath "$buildDir/$buildName"


Remove-Item -Path "$buildDir/Tmp" -Recurse -Force 
Write-Output "file $buildDir/$buildName created successfully" 