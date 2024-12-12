# �N��e�ؿ��]�w���ܼ�
$currentDir = Get-Location

try {
    # �j�M�ثe�ؿ����H FFXIVMacroController_v �}�Y�� ZIP �ɮ�
    $zipFiles = Get-ChildItem -Path $currentDir -Filter "FFXIVMacroController_v*.zip"

    if ($zipFiles.Count -eq 0) {
        Write-Host "��������ŦX���� ZIP �ɮסC"
        return
    }

    foreach ($zipFile in $zipFiles) {
        # �����Y�ؼи��|����e�ؿ�
        $destinationPath = $currentDir

        Write-Host "���b�����Y�ɮ�: $($zipFile.FullName) �� $destinationPath"

        try {
            # �ϥ� Expand-Archive �����Y���л\�w�s�b���ɮ�
            Expand-Archive -Path $zipFile.FullName -DestinationPath $destinationPath -Force
            Write-Host "���\�����Y: $($zipFile.Name)"
        } catch {
            Write-Host "�����Y�ɵo�Ϳ��~: $_"
        }
    }

    # �R���Ҧ��H FFXIVMacroController_v �}�Y�� ZIP �ɮ�
    foreach ($zipFile in $zipFiles) {
        try {
            Remove-Item -Path $zipFile.FullName -Force
            Write-Host "�w�R���ɮ�: $($zipFile.FullName)"
        } catch {
            Write-Host "�R���ɮ׮ɵo�Ϳ��~: $_"
        }
    }
} catch {
    Write-Host "����L�{���o�ͥ��w�������~: $_"
}

# ����3�������
Start-Sleep -Seconds 3
