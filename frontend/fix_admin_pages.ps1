$files = Get-ChildItem -Path "src\app\pages" -Recurse -Include "*.component.ts", "*.component.html"

foreach ($f in $files) {
    $content = Get-Content $f.FullName -Raw
    $original = $content

    if ($f.Extension -eq ".ts") {
        # Remove imports from shared
        $content = $content -replace "import\s+\{([^}]*?)BocPageHeroComponent([^}]*?)\}\s+from\s+['`"].*?['`"];?`r?`n?", "import {$1 $2} from '../../../shared';`r`n"
        $content = $content -replace "import\s+\{([^}]*?)BocGlassCardComponent([^}]*?)\}\s+from\s+['`"].*?['`"];?`r?`n?", "import {$1 $2} from '../../../shared';`r`n"
        $content = $content -replace "import\s+\{([^}]*?)BocStatCardComponent([^}]*?)\}\s+from\s+['`"].*?['`"];?`r?`n?", "import {$1 $2} from '../../../shared';`r`n"
        $content = $content -replace "import\s+\{([^}]*?)BocEmptyStateComponent([^}]*?)\}\s+from\s+['`"].*?['`"];?`r?`n?", "import {$1 $2} from '../../../shared';`r`n"
        
        # Cleanup empty imports
        $content = $content -replace "import\s+\{\s*\}\s+from\s+['`"].*?['`"];?`r?`n?", ""
        $content = $content -replace "import\s+\{\s*,\s*\}\s+from\s+['`"].*?['`"];?`r?`n?", ""
        
        # Remove from @Component imports array
        $content = $content -replace "BocPageHeroComponent\s*,?", ""
        $content = $content -replace "BocGlassCardComponent\s*,?", ""
        $content = $content -replace "BocStatCardComponent\s*,?", ""
        $content = $content -replace "BocEmptyStateComponent\s*,?", ""
        
        # Remove trailing commas in imports array
        $content = $content -replace ",\s*\]", "]"
    }

    if ($f.Extension -eq ".html") {
        # Replace boc-glass-card
        $content = [regex]::Replace($content, '(?s)<boc-glass-card\s+title="([^"]+)">([\s\S]*?)<\/boc-glass-card>', '<div class="card mb-4"><div class="card-header"><span class="text-h3">$1</span></div><div class="card-body">$2</div></div>')
        $content = [regex]::Replace($content, '(?s)<boc-glass-card>([\s\S]*?)<\/boc-glass-card>', '<div class="card mb-4"><div class="card-body">$1</div></div>')

        # Replace boc-page-hero
        $content = [regex]::Replace($content, '(?s)<boc-page-hero\s+title="([^"]+)"\s+subtitle="([^"]+)".*?>([\s\S]*?)<\/boc-page-hero>', '<div class="page-header"><div><h1 class="text-h1">$1</h1><p class="text-label">$2</p></div>$3</div>')
        $content = [regex]::Replace($content, '(?s)<boc-page-hero\s+title="([^"]+)".*?>([\s\S]*?)<\/boc-page-hero>', '<div class="page-header"><div><h1 class="text-h1">$1</h1></div>$2</div>')

        # Replace boc-stat-card
        $content = [regex]::Replace($content, '(?s)<boc-stat-card\s+label="([^"]+)"\s+\[value\]="([^"]+)".*?><\/boc-stat-card>', '<div class="kpi-card"><div class="kpi-header"><span class="text-upper-label">$1</span></div><div class="kpi-value">{{ $2 }}</div></div>')
        $content = [regex]::Replace($content, '(?s)<boc-stat-card\s+label="([^"]+)"\s+value="([^"]+)".*?><\/boc-stat-card>', '<div class="kpi-card"><div class="kpi-header"><span class="text-upper-label">$1</span></div><div class="kpi-value">$2</div></div>')

        # Replace boc-empty-state
        $content = [regex]::Replace($content, '(?s)<boc-empty-state\s+icon="([^"]+)"\s+title="([^"]+)"\s+message="([^"]+)".*?>([\s\S]*?)<\/boc-empty-state>', '<div class="empty-state"><div class="empty-icon"><i class="bi $1"></i></div><h3>$2</h3><p>$3</p>$4</div>')
    }

    if ($content -ne $original) {
        Set-Content -Path $f.FullName -Value $content
        Write-Host "Updated $($f.FullName)"
    }
}
