param(
    [string[]]$FilesToZip,
    [string]$DestinationDirectory
)

# 7Zipの実行ファイルへのパス（必要に応じて変更してください）
$7ZipPath = "C:/Program Files/7-Zip/7z.exe"

# ZIPファイルの名前を生成（現在のタイムスタンプを使用）
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$zipFileName = "Archive_$timestamp.zip"
$zipFilePath = Join-Path $DestinationDirectory $zipFileName

# ファイルの一時リストを作成
$tempFileList = "./filelist.txt"
$FilesToZip | Out-File $tempFileList

# 7Zipを使用してファイルをZIP形式で圧縮
& $7ZipPath a -tzip $zipFilePath $FilesToZip -scsUTF-8

# 一時ファイルリストを削除
Remove-Item $tempFileList
