# دليل التشغيل المحلي للتطوير (BOC Local Dev Runbook)

> **النظام:** BOC Research & Evaluation System  
> **البيئة:** Windows — تطوير محلي بدون Docker  
> **آخر تحديث:** 2026-05-23 — تم التحقق من هذه الخطوات وتجربتها بنجاح

---

## 1. المتطلبات الأساسية (Prerequisites)

تأكد من توفر البرامج التالية قبل البدء:

| البرنامج | الإصدار المطلوب | ملاحظة |
|---|---|---|
| **.NET SDK** | 9.0 أو 10.0 | `dotnet --version` للتحقق |
| **Node.js** | 18+ (مُختبر على 25) | `node --version` للتحقق |
| **npm** | 9+ | يأتي مع Node.js |
| **SQL Server** | 2019+ (Developer أو Express) | يجب أن يكون يعمل |
| **sqlcmd** | أي إصدار | للتحقق من قاعدة البيانات |
| **Redis** | اختياري | مطلوب لـ SignalR — راجع الملاحظات |

---

## 2. إعداد قاعدة البيانات (Database Setup)

### أ. التأكد من الاتصال بـ SQL Server

سلسلة الاتصال الحالية (التي تم التحقق منها) في:
`d:\WebApps\BOC_Research_System\backend\BOC.WebAPI\appsettings.json`

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=127.0.0.1,1433;Database=BOC_Research_Evaluation;User Id=sa;Password=12345;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False;",
  "RedisCache": "localhost:6379"
}
```

> [!IMPORTANT]
> تأكد من أن SQL Server يعمل على المنفذ `1433` وأن بيانات الاعتماد `sa / 12345` صحيحة.  
> للتحقق من الاتصال:
> ```powershell
> sqlcmd -S "127.0.0.1,1433" -U sa -P "12345" -Q "SELECT DB_NAME();"
> ```

### ب. التحقق من وجود قاعدة البيانات والجداول

```powershell
sqlcmd -S "127.0.0.1,1433" -U sa -P "12345" -Q "SELECT name FROM sys.databases WHERE name = 'BOC_Research_Evaluation';"
```

إذا لم تكن قاعدة البيانات موجودة، طبّق الـ Migrations:

```powershell
cd d:\WebApps\BOC_Research_System\backend
dotnet ef database update --project BOC.Infrastructure --startup-project BOC.WebAPI
```

### ج. التأكد من وجود مستخدم Admin

```powershell
sqlcmd -S "127.0.0.1,1433" -U sa -P "12345" -Q "SELECT Email, AccountStatus, TwoFactorEnabled FROM BOC_Research_Evaluation.dbo.AppUsers;"
```

---

## 3. تشغيل الخادم الخلفي (Backend — ASP.NET Core)

> [!IMPORTANT]
> يجب تشغيل الـ Backend **أولاً** قبل الـ Frontend.

### الخطوة 1: ثق بشهادة HTTPS للتطوير (مرة واحدة فقط)

```powershell
dotnet dev-certs https --trust
```

إذا طلب منك تأكيد تثبيت الشهادة، اختر **نعم**.

### الخطوة 2: تشغيل الـ Backend

افتح **Terminal جديد** وشغّل:

```powershell
dotnet run --project d:\WebApps\BOC_Research_System\backend\BOC.WebAPI --launch-profile "https"
```

### ✅ علامات النجاح

ستظهر رسائل مشابهة لهذه:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7139
      Now listening on: http://localhost:5139
Application started.
```

> [!NOTE]
> - الـ API يعمل على: **`https://localhost:7139`**
> - Swagger متاح على: **`https://localhost:7139/swagger`**
> - **تحذير Redis:** ستظهر رسالة `Not connected to Redis` — هذا طبيعي إذا لم يكن Redis مثبتاً. النظام يعمل بدونه مع تعطّل بعض ميزات SignalR فقط.

---

## 4. تشغيل الواجهة الأمامية (Frontend — Angular)

افتح **Terminal جديداً آخر** (منفصل عن الـ Backend) وشغّل:

### الخطوة 1: تثبيت الحزم (مرة واحدة أو بعد أي تغيير في package.json)

```powershell
cd d:\WebApps\BOC_Research_System\frontend
npm install --legacy-peer-deps
```

### الخطوة 2: تشغيل خادم التطوير

```powershell
npm start
```

### ✅ علامات النجاح

```
Application bundle generation complete.
Watch mode enabled. Watching for file changes...
  ➜  Local:   http://localhost:4200/
```

> [!NOTE]
> - الواجهة الأمامية تعمل على: **`http://localhost:4200`**
> - صفحة تسجيل الدخول: **`http://localhost:4200/auth/login`**

---

## 5. تسجيل الدخول للمرة الأولى

### بيانات الدخول (Admin)

| الحقل | القيمة |
|---|---|
| **البريد الإلكتروني** | `hussainkn1992@gmail.com` |
| **كلمة المرور** | `Admin@123` |

### سير العملية عند أول دخول

1. أدخل بيانات الدخول واضغط **تسجيل الدخول**
2. سيتم توجيهك لصفحة **إعداد المصادقة الثنائية (2FA)**
3. امسح رمز **QR Code** الظاهر باستخدام **Google Authenticator** أو **Microsoft Authenticator**
4. أدخل الكود المكوّن من **6 أرقام** من التطبيق
5. اضغط **تأكيد الرمز** — سيتم تفعيل 2FA وتسجيل دخولك تلقائياً

> [!IMPORTANT]
> في حال احتجت لإعادة ضبط 2FA (مثلاً: تغيير الجهاز):
> ```powershell
> sqlcmd -S "127.0.0.1,1433" -U sa -P "12345" -Q "SET QUOTED_IDENTIFIER ON; UPDATE [BOC_Research_Evaluation].[dbo].[AppUsers] SET TwoFactorEnabled = 0, TwoFactorSecret = NULL WHERE Email = 'hussainkn1992@gmail.com';"
> ```
> ثم سجّل دخولك مجدداً وأعد مسح الـ QR Code.

---

## 6. إيقاف النظام

لإيقاف الـ Backend أو الـ Frontend، اضغط **`Ctrl + C`** في كل Terminal على حدة.

---

## 7. ملاحظات هامة

### Redis (اختياري للتطوير)
- Redis مطلوب لميزة **SignalR** (الإشعارات الفورية وغرف الاجتماعات).
- بدون Redis يعمل النظام بشكل طبيعي ماعدا الإشعارات الفورية.
- لتشغيل Redis على Windows:
  ```powershell
  # باستخدام Docker (الأسهل):
  docker run -d -p 6379:6379 --name redis redis:latest
  # أو تثبيت Memurai (بديل Redis لـ Windows)
  ```

### منافذ النظام (Ports)

| الخدمة | البروتوكول | المنفذ |
|---|---|---|
| Angular Frontend | HTTP | `4200` |
| ASP.NET Core API | HTTPS | `7139` |
| ASP.NET Core API | HTTP | `5139` |
| SQL Server | TCP | `1433` |
| Redis | TCP | `6379` |

### CORS
الـ Backend مُعدّ للسماح بطلبات من `http://localhost:4200`. إذا غيّرت منفذ الـ Frontend يجب تحديث إعدادات CORS في `Program.cs`.

### SignalR Hubs
مسار الـ Hub: `https://localhost:7139/hubs/notifications`

---

## 8. استكشاف الأخطاء الشائعة (Troubleshooting)

| المشكلة | السبب المحتمل | الحل |
|---|---|---|
| `ERR_CONNECTION_REFUSED` على `4200` | الـ Frontend لم يُشغَّل | شغّل `npm start` في مجلد `frontend` |
| `ERR_CONNECTION_REFUSED` على `7139` | الـ Backend لم يُشغَّل | شغّل أمر `dotnet run` |
| `Unknown Error / status 0` | الـ Backend غير متاح | تأكد أن الـ Backend يعمل على `https://localhost:7139` |
| خطأ شهادة HTTPS في المتصفح | الشهادة لم يُوثق بها | شغّل `dotnet dev-certs https --trust` |
| صفحة 2FA لا تُعيد لها | قاعدة البيانات لا تحتوي على المستخدم | تحقق من وجود المستخدم وحالته (`AccountStatus = Active`) |
| رمز QR لا يظهر | مشكلة في التوليد | راجع Console المتصفح — النظام يولّد الـ QR محلياً بدون إنترنت |
| `Not connected to Redis` | Redis غير مثبت | طبيعي في بيئة التطوير — لا يؤثر على الدخول وإدارة البيانات |
