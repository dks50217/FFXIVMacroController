# 將當前目錄設定為變數
$currentDir = Get-Location

try {
    # 搜尋目前目錄中以 FFXIVMacroController_v 開頭的 ZIP 檔案
    $zipFiles = Get-ChildItem -Path $currentDir -Filter "FFXIVMacroController_v*.zip"

    if ($zipFiles.Count -eq 0) {
        Write-Host "未找到任何符合條件的 ZIP 檔案。"
        return
    }

    foreach ($zipFile in $zipFiles) {
        # 解壓縮目標路徑為當前目錄
        $destinationPath = $currentDir

        Write-Host "正在解壓縮檔案: $($zipFile.FullName) 至 $destinationPath"

        try {
            # 使用 Expand-Archive 解壓縮並覆蓋已存在的檔案
            Expand-Archive -Path $zipFile.FullName -DestinationPath $destinationPath -Force
            Write-Host "成功解壓縮: $($zipFile.Name)"
        } catch {
            Write-Host "解壓縮時發生錯誤: $_"
        }
    }

    # 刪除所有以 FFXIVMacroController_v 開頭的 ZIP 檔案
    foreach ($zipFile in $zipFiles) {
        try {
            Remove-Item -Path $zipFile.FullName -Force
            Write-Host "已刪除檔案: $($zipFile.FullName)"
        } catch {
            Write-Host "刪除檔案時發生錯誤: $_"
        }
    }
} catch {
    Write-Host "執行過程中發生未預期的錯誤: $_"
}

# 等待3秒後關閉
Start-Sleep -Seconds 3
