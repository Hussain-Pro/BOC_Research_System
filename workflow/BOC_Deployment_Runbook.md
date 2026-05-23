# دليل تشغيل ونشر تطبيق BOC Research System (Deployment Runbook)

يوضح هذا الدليل الخطوات التفصيلية لنشر تطبيق إدارة البحوث لشركة نفط البصرة (BOC) باستخدام حاويات `Docker` لضمان بيئة إنتاج مستقرة وقابلة للتوسع.

## 1. البنية التحتية للحاويات (Docker Architecture)

يعتمد التطبيق على بنية ميكروسيرفس مصغرة باستخدام `docker-compose.yml` الذي ينشئ 4 حاويات رئيسية:
1. **db (SQL Server 2022):** قاعدة البيانات الأساسية للنظام.
2. **redis:** الكاش والذاكرة المؤقتة، يُستخدم لدعم نظام `SignalR` (Backplane) وإدارة الجلسات السريعة وحماية Rate Limiting.
3. **backend (ASP.NET Core 10):** طبقة الـ API الخاصة بالنظام ومعالجة الأعمال.
4. **frontend (Angular 17 + NGINX):** واجهة المستخدم، حيث يعمل `NGINX` أيضاً كـ Reverse Proxy لتوجيه طلبات الـ API والـ WebSockets إلى الـ Backend.

---

## 2. متطلبات التشغيل الأساسية (Prerequisites)
قبل البدء بالنشر، تأكد من توفر الآتي على الخادم:
* نظام تشغيل Linux (مثل Ubuntu 22.04 LTS) أو Windows Server مع WSL2.
* تثبيت `Docker` و `Docker Compose` (الإصدار 2.0 فما فوق).
* فتح البورتات التالية في الجدار الناري (Firewall):
  * **80** (HTTP - للواجهة الأمامية)
  * **443** (HTTPS - في حال تفعيل شهادة SSL)

---

## 3. إعداد البيئة والتشغيل الأول (First Run)

### أ. سحب الكود وإعداد المتغيرات
قم بوضع الكود المصدري للمشروع في مسار على الخادم (مثال: `/opt/boc-research`).
راجع ملف `docker-compose.yml` وتأكد من تحديث كلمات المرور الافتراضية التالية ببيانات حقيقية وآمنة:
* `MSSQL_SA_PASSWORD`: كلمة مرور مسؤول قاعدة البيانات.
* `JwtSettings__Secret`: المفتاح السري لتشفير التوكن.

### ب. بناء وتشغيل الحاويات
افتح الطرفية (Terminal) في مسار المشروع وقم بتنفيذ الأمر التالي לבناء الصور وتشغيل النظام في الخلفية:
```bash
docker-compose up -d --build
```
*سيقوم هذا الأمر بـ:*
1. سحب صور SQL و Redis الرسمية.
2. بناء Backend (عملية `dotnet publish`).
3. بناء Frontend (عملية `npm run build`).
4. تشغيل جميع الحاويات وربطها بنفس الشبكة.

### ج. التحقق من الحالة
بعد دقيقتين، تحقق من حالة الحاويات لضمان عملها بشكل صحي (Healthy):
```bash
docker-compose ps
```

---

## 4. ترحيل قاعدة البيانات (Database Migrations)

نظراً لأن قاعدة البيانات يتم إنشاؤها جديدة ضمن الحاوية، ستحتاج إلى تطبيق الـ Migrations.
بما أن الـ WebAPI مصمم للاتصال بقاعدة البيانات، يمكنك إما جعل التطبيق يقوم بالتحديث التلقائي عند بدء التشغيل في بيئة الإنتاج (`dbContext.Database.Migrate()`)، أو يمكنك تطبيق التحديثات يدوياً عبر أداة `dotnet ef`:

```bash
# استخدام حاوية الباك إند لتنفيذ الـ Migration (بافتراض وجود الأدوات)
docker exec -it <backend_container_name> dotnet ef database update
```

*(تنويه: يفضل إضافة كود `context.Database.Migrate()` في ملف `Program.cs` ليتم التحديث تلقائياً عند تشغيل حاوية الـ Backend).*

---

## 5. النسخ الاحتياطي والصيانة (Backup & Maintenance)

### النسخ الاحتياطي لقاعدة البيانات
لأخذ نسخة احتياطية من قاعدة البيانات `SQL Server` بدون إيقاف الحاويات:
```bash
docker exec -it <db_container_name> /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P "YourStrong!Password2026" \
   -Q "BACKUP DATABASE [BOCResearchDb] TO DISK = N'/var/opt/mssql/backup/BOC_Full.bak' WITH NOFORMAT, NOINIT, NAME = 'BOC-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"
```
سيتم حفظ الملف في المجلد المشترك (Volume) الخاص بـ `sql_data`.

### تحديث النظام
عند إجراء تعديلات على الكود (تحديث الواجهة أو الباك إند)، يمكنك نشر التحديث بدون فترة توقف طويلة:
```bash
git pull origin main
docker-compose up -d --build
```
سيقوم Docker بإعادة بناء الحاويات التي تم تعديلها فقط واستبدال القديمة بسلاسة.

---

## 6. الخطوات المستقبلية (CI/CD Pipeline)

لأتمتة هذه العملية لاحقاً باستخدام **Azure DevOps** أو **GitHub Actions**:
1. إعداد Pipeline يقوم ببناء صورتي الـ `backend` والـ `frontend` عند كل دمج لفرع الـ `main`.
2. رفع الصور إلى سجل حاويات خاص (مثل Azure Container Registry أو خادم Harbor داخلي).
3. استخدام أداة مثل `Watchtower` لتحديث حاويات الإنتاج تلقائياً بمجرد توفر صور جديدة، أو إضافة خطوة `SSH` في الـ Pipeline لتنفيذ أمر `docker-compose pull && docker-compose up -d`.
