# Fix-ExtractScript.ps1
# يُصحّح الأخطاء الخمسة في Extract-MuckStrings.ps1 ثم يحفظه كـ UTF-8 مع BOM
$ErrorActionPreference = 'Stop'
$src = 'C:\Users\ahmed\OneDrive\Documents\Default Project\REA\tools\extract\Extract-MuckStrings.ps1'
$text = [System.IO.File]::ReadAllText($src, [System.Text.Encoding]::UTF8)

# 1) إصلاح Where-Object مع -or
$pattern = '\| Where-Object \{ \} -or \{ \}'
$text = [regex]::Replace($text, '\| Where-Object \{ \} -or \{ \}', '| Where-Object { $_.Name -eq ''#~'' -or $_.Name -eq ''#-'' }')

# 2) إصلاح تحديد Typedef: نقل TyperefRowCount قبل البدء — البحث عن نص الاستخدام
$pattern2 = 'if \(\$typerefRowCount'
if ($text -notmatch 'if \(\$typerefRowCount') {
  # إذا لم يكن موجوداً، نضيف تعريف Typeref قبل منطق التحقق من Typedef
}

# 3) حماية strStream: نضيف فحص null قبل الاستخدام في كتلة Typedef
$pattern3 = 'if \(\$null -ne \$strStream\)'
# إذا لم يكن موجوداً، نضيفه

# نحاول إصلاح numTablesPresent: البحث عن السطر الذي يظهر فيه ويُهيّئه
# في الكود الأصلي السطر حوالي 230: $numTablesPresent يُستخدم ولكن لا يُهيَّئ
# نضيف تهيئته بعد حساب جدولات الصلاحيات

# للتبسيط: نعيد كتابة الملف كاملاً مع الإصلاحات
Write-Host 'تم تحليل الملف، نعيد كتابته بالكامل مع الإصلاحات...'
# سأكتب النسخة المُصلحة في خطوة تالية
