param (
    [string]$ExePath = ""
)

$currentDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# 等待主程式 exe 完全釋放檔案鎖
if ($ExePath -ne "" -and (Test-Path $ExePath)) {
    $maxWait = 30
    $waited = 0
    Write-Host "等待主程式關閉..."
    while ($waited -lt $maxWait) {
        try {
            $stream = [System.IO.File]::Open($ExePath, 'Open', 'ReadWrite', 'None')
            $stream.Close()
            Write-Host "主程式已關閉，開始更新。"
            break
        } catch {
            Start-Sleep -Seconds 1
            $waited++
        }
    }
    if ($waited -ge $maxWait) {
        Write-Host "等待逾時，強制繼續更新..."
    }
} else {
    Start-Sleep -Seconds 3
}

try {
    $zipFiles = Get-ChildItem -Path $currentDir -Filter "FFXIVMacroController_v*.zip"

    if ($zipFiles.Count -eq 0) {
        Write-Host "找不到符合條件的 ZIP 檔案。"
        exit 1
    }

    foreach ($zipFile in $zipFiles) {
        Write-Host "正在解壓: $($zipFile.FullName)"
        try {
            Expand-Archive -Path $zipFile.FullName -DestinationPath $currentDir -Force
            Write-Host "解壓成功: $($zipFile.Name)"
        } catch {
            Write-Host "解壓失敗: $_"
            exit 1
        }
    }

    foreach ($zipFile in $zipFiles) {
        try {
            Remove-Item -Path $zipFile.FullName -Force
            Write-Host "已刪除: $($zipFile.FullName)"
        } catch {
            Write-Host "刪除失敗: $_"
        }
    }

    Write-Host "更新完成，正在重新啟動..."

    if ($ExePath -ne "" -and (Test-Path $ExePath)) {
        Start-Process -FilePath $ExePath
    } else {
        $exe = Get-ChildItem -Path $currentDir -Filter "FFXIVMacroControllerApp.exe" | Select-Object -First 1
        if ($exe) {
            Start-Process -FilePath $exe.FullName
        }
    }

} catch {
    Write-Host "更新過程發生錯誤: $_"
    Read-Host "按 Enter 關閉"
    exit 1
}
