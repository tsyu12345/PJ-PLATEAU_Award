# PowerShell スクリプト: 指定されたファイルをZIPにして特定のフォルダにコピー

# ZIP化するファイルのパス
$sourceFile = "PLATEAU-run Client\Assets\Scenes\PLATEAU-TEST.unity"

# ZIPファイルを保存するパス
$zipFile = "./temp/Scenes.zip"

# ZIPファイルをコピーする先のフォルダ
$destinationFolder = "G:\マイドライブ\MiyayuLab\smartcity\PLATEAU AWARD\LFS"

# 指定されたファイルをZIPファイルに圧縮
Compress-Archive -Path $sourceFile -DestinationPath $zipFile

# ZIPファイルを指定されたフォルダにコピー
Copy-Item -Path $zipFile -Destination $destinationFolder

# 確認メッセージ（オプション）
Write-Host "ファイルがZIP化され、指定されたフォルダにコピーされました。"
