if (Test-Path .\Source\Pawnmorphs\.vs) {
    Remove-Item -Force .\Source\Pawnmorphs\.vs 
}

if (Test-Path .\Source\Pawnmorphs\Esoteria\obj) {
    Remove-Item -Force .\Source\Pawnmorphs\Esoteria\obj
}