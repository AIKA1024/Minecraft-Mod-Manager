# 设置工作目录
Split-Path -Parent $MyInvocation.MyCommand.Path | Set-Location
$csprojPath = Join-Path $PSScriptRoot "MAZDA_MCTool\MAZDA_MCTool.csproj"

# 加载 .csproj
[xml]$proj = Get-Content $csprojPath

# 查找第一个 <Version> 节点
$versionNode = $proj.SelectSingleNode('//Version')

if (-not $versionNode) {
    Write-Host "未找到 <Version>，添加并设置为 1.0.0（不进行自增）"
    $targetGroup = $proj.Project.PropertyGroup[0]
    $versionNode = $proj.CreateElement("Version", $proj.DocumentElement.NamespaceURI)
    $versionNode.InnerText = "1.0.0"
    $targetGroup.AppendChild($versionNode) | Out-Null
    $newVersionString = "1.0.0"
}
else {
    # 自增 patch
    $versionText = $versionNode.InnerText
    $versionParts = $versionText -split '\.'
    $major = [int]$versionParts[0]
    $minor = [int]$versionParts[1]
    $patch = if ($versionParts.Count -ge 3) { [int]$versionParts[2] } else { 0 }
    $patch += 1
    $newVersionString = "$major.$minor.$patch"
    Write-Host "更新版本号为: $newVersionString"
    $versionNode.InnerText = $newVersionString
}

# 保存 .csproj
$proj.Save($csprojPath)

Remove-Item -Path "./VelopackPublish/*" -Force
# Remove-Item -Path "./Releases/*" -Force
dotnet publish ./MAZDA_MCTool/MAZDA_MCTool.csproj -c Release -r win-x64 -o ./VelopackPublish/
vpk pack -u MAZDA_MCTool -v $newVersionString  -p ./VelopackPublish -e "MAZDA_MCTool.exe"