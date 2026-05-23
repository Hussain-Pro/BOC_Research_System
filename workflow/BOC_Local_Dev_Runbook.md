# دليل التشغيل المحلي للتطوير (Local Development Runbook)

هذا الدليل مخصص للمطورين الذين يرغبون بتشغيل مشروع **BOC Research System** على أجهزتهم الشخصية (Windows) لأغراض التطوير، التعديل، والتحقق المباشر بدون الحاجة لاستخدام `Docker`.

---

## 1. المتطلبات الأساسية (Prerequisites)
تأكد من توفر البرامج التالية على حاسبتك:
1. **.NET 10.0 SDK:** لتشغيل وبناء طبقة الـ Backend.
2. **Node.js (الإصدار 20+ أو 25 كما هو مثبت لديك):** لتشغيل واجهة Angular.
3. **Angular CLI:** (اختياري) يمكن تثبيته عبر `npm install -g @angular/cli`.
4. **SQL Server:** يفضل إصدار Developer أو Express (أو LocalDB).
5. **Redis:** للعمل على الـ SignalR والكاش. (بالنسبة لـ Windows، يمكنك تشغيل Redis باستخدام `WSL`، أو عبر حاوية Docker صغيرة `docker run -d -p 6379:6379 redis`، أو استخدام بديل كـ Memurai).

---

## 2. إعداد قاعدة البيانات (Database Setup)

### أ. ضبط سلسلة الاتصال (Connection String)
تحتاج إلى التأكد من أن الـ Backend يشير إلى خادم الـ SQL المحلي الخاص بك.
افتح ملف:
`d:\WebApps\BOC_Research_System\backend\BOC.WebAPI\appsettings.Development.json`

وتأكد من أن الـ `DefaultConnection` تشير إلى السيرفر المحلي. (مثال):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BOCResearchDb_Dev;Trusted_Connection=True;MultipleActiveResultSets=true",
  "RedisCache": "localhost:6379"
}
```
*(ملاحظة: عدل `Server=` لتطابق اسم خادم SQL في جهازك إذا لم تكن تستخدم `localdb`).*

### ب. تنفيذ الترحيل (EF Core Migrations)
لإنشاء الجداول في قاعدة البيانات المحلية، افتح `PowerShell` أو `Terminal` وقم بالتالي:
```powershell
cd d:\WebApps\BOC_Research_System\backend
dotnet ef database update --project BOC.Infrastructure --startup-project BOC.WebAPI
```
هذا سيقوم بتطبيق هيكلية الجداول وتكوين الـ Always Encrypted.

---

## 3. تشغيل الخادم (Backend - ASP.NET Core)

1. افتح موجه الأوامر (Terminal).
2. انتقل إلى مجلد `WebAPI`:
```powershell
cd d:\WebApps\BOC_Research_System\backend\BOC.WebAPI
```
3. قم بتشغيل التطبيق في وضع التطوير:
```powershell
dotnet run
```
4. بعد التشغيل بنجاح، يمكنك الوصول إلى واجهة الفحص (Swagger) من خلال المتصفح عبر الرابط:
`http://localhost:5000/swagger` أو `https://localhost:5001/swagger` (تأكد من رقم البورت الذي سيظهر في الشاشة).

---

## 4. تشغيل الواجهة الأمامية (Frontend - Angular)

1. افتح موجه أوامر **جديد** (Terminal آخر).
2. انتقل إلى مجلد الـ Frontend:
```powershell
cd d:\WebApps\BOC_Research_System\frontend
```
3. (اختياري إذا كانت هذه أول مرة) قم بتثبيت الحزم:
```powershell
npm install --legacy-peer-deps
```
4. قم بتشغيل خادم تطوير Angular:
```powershell
npm start
```
أو
```powershell
ng serve -o
```
5. سيبدأ تشغيل الواجهة الأمامية ويفتح المتصفح تلقائياً على الرابط:
`http://localhost:4200`

---

## 5. ملاحظات هامة أثناء التطوير

- **CORS:** واجهة التطوير (Angular) تعمل على بورت `4200`. تأكد من أن إعدادات `CORS` في الباك إند (`Program.cs`) تسمح باستقبال الطلبات من `http://localhost:4200`.
- **SignalR:** سيحاول الـ Frontend الاتصال بالـ WebSockets على مسار `/hubs/notifications`. تأكد من استخدام الرابط الكامل للباك إند المحلي عند بناء الاتصال (مثلاً: `http://localhost:5000/hubs/notifications`).
- **تخطي Redis مؤقتاً:** إذا كنت تواجه مشكلة في تشغيل Redis محلياً للـ SignalR أثناء التطوير فقط، يمكنك التعليق (Comment) على السطر `builder.Services.AddSignalR().AddStackExchangeRedis(...)` واستخدام الـ In-Memory SignalR في بيئة التطوير.
