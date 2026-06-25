import os
import re

files = [
    r'Assets\Scripts\LevelRewardManager.cs',
    r'Assets\Scripts\LevelManager.cs',
    r'Assets\Scripts\CrazyGamesManager.cs',
    r'Assets\Scripts\CGRewardManager.cs'
]

for f in files:
    try:
        with open(f, 'r', encoding='utf-8', errors='ignore') as file:
            content = file.read()
    except Exception as e:
        print('Could not read', f, e)
        continue
    
    if 'LevelRewardManager.cs' in f:
        # We can just wrap the whole Awake/Start related logic or do simple replacements
        content = re.sub(r'if \(CrazyGamesManager\.Instance != null\)', '// if (CrazyGamesManager.Instance != null)', content)
        content = re.sub(r'CrazyGamesManager\.Instance\.levelRewardManager = this;', '// CrazyGamesManager.Instance.levelRewardManager = this;', content)
        content = re.sub(r'Debug\.Log\(\"LevelRewardManager ba.*?\"\);', '// Debug.Log(\"LevelRewardManager ba...\");', content)
        content = re.sub(r'else\s*{\s*Debug\.LogWarning\(\"CrazyGamesManager sahnede bulunamad.*?\"\);\s*}', '/* else { Debug.LogWarning(\"CrazyGamesManager sahnede bulunamad...\"); } */', content)

    if 'LevelManager.cs' in f:
        content = re.sub(r'CrazyGamesManager cgManager = CrazyGamesManager\.Instance;', '// CrazyGamesManager cgManager = CrazyGamesManager.Instance;', content)
        content = re.sub(r'if \(cgManager == null\)', '// if (cgManager == null)', content)
        content = re.sub(r'cgManager = FindFirstObjectByType<CrazyGamesManager>\(\);', '// cgManager = FindFirstObjectByType<CrazyGamesManager>();', content)
        content = re.sub(r'if \(cgManager != null\)', 'if (false) // if (cgManager != null)', content)
        content = re.sub(r'cgManager\.ShowMidgameAd', '// cgManager.ShowMidgameAd', content)

    if 'CrazyGamesManager.cs' in f:
        content = re.sub(r'using CrazyGames;', '// using CrazyGames;', content)
        content = re.sub(r'CrazySDK\.Init\(\(\) =>', '/* CrazySDK.Init(() =>', content)
        content = re.sub(r'Debug\.Log\(\"CrazyGames SDK Ready\"\);\s*}\);', 'Debug.Log(\"CrazyGames SDK Ready\");\n        }); */', content)
        content = re.sub(r'CrazySDK\.Ad\.RequestAd', '// CrazySDK.Ad.RequestAd', content)

    with open(f, 'w', encoding='utf-8') as file:
        file.write(content)
