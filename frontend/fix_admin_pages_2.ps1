$files = Get-ChildItem -Path "src\app\pages" -Recurse -Include "*.component.html"

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    $original = $content

    # Replace boc-glass-card with ANY attributes
    $content = [regex]::Replace($content, '(?s)<boc-glass-card\s+title="([^"]+)"[^>]*>([\s\S]*?)<\/boc-glass-card>', '<div class="card mb-4"><div class="card-header"><span class="text-h3">$1</span></div><div class="card-body">$2</div></div>')
    $content = [regex]::Replace($content, '(?s)<boc-glass-card[^>]*>([\s\S]*?)<\/boc-glass-card>', '<div class="card mb-4"><div class="card-body">$1</div></div>')

    # Replace boc-empty-state with ANY attributes
    $content = [regex]::Replace($content, '(?s)<boc-empty-state[^>]*>([\s\S]*?)<\/boc-empty-state>', '<div class="empty-state" style="padding:40px"><div class="empty-icon"><i class="bi bi-info-circle"></i></div><h3>لا توجد بيانات</h3><p>البيانات المطلوبة غير متوفرة حالياً</p>$1</div>')

    if ($content -ne $original) {
        Set-Content -Path $f.FullName -Value $content
        Write-Host "Updated $($f.FullName)"
    }
}
