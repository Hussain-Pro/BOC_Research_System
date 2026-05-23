# تحليل شاشات واجهة المستخدم (UI/UX Screens Analysis)

بناءً على ملف `BOC_Research_System_v11_Unified_Master_Blueprint.md` (والذي يحتوي على 33 شاشة أساسية)، قمت بمقارنة المتطلبات مع ما تم إنجازه فعلياً في مجلد `frontend/src/app/pages`. إليك التفصيل الكامل:

## 🟢 أولاً: الشاشات المنجزة (موجودة حالياً في النظام)
1. **Secure Login Screen** ➔ `login`
2. **Employee Sign-Up Screen** ➔ `register`
3. **Password Reset Portal** ➔ `forgot-password` & `reset-password`
4. **Two-Factor Authentication (2FA)** ➔ `two-factor`
5. **Research Submission Wizard** ➔ `submit-research`
6. **Interactive Research Timeline Viewer** ➔ `research-timeline`
7. **Incoming Research Triage Dashboard** ➔ `triage-dashboard`
8. **Evaluator Roster & Status Manager** ➔ `admin/evaluator-roster`
9. **System Master Audit Log Viewer** ➔ `admin/audit-logs`
10. **Meeting Portal RSVP Screen** ➔ `rsvp`
11. **Meeting Minutes Studio & Live Editor** ➔ `meeting-studio`
12. **Committee Internal Chat Box** ➔ `chat`
13. **In-App Notification Sidebar** ➔ `notifications`
14. **Future-Proof Ministry Gateway** ➔ `admin/ministry-gateway`
15. **Executive Analytics Dashboard** ➔ `admin/analytics-dashboard`
*(ملاحظة: شاشة `system-config` موجودة أيضاً لإدارة إعدادات النظام).*

---

## 🔴 ثانياً: الشاشات المفقودة (لم يتم بناؤها بعد حسب הـ Blueprint)

### 👤 إدارة الهوية والمستخدمين
- **HR Verification Approval Dashboard**: لوحة لمسؤولي الموارد البشرية للموافقة على الحسابات الجديدة.
- **User Profile Page**: صفحة الملف الشخصي للمستخدم (تتضمن أدواره، بياناته، وصلاحياته المفوضة).
- **System Preferences Panel**: لوحة تفضيلات المستخدم (لتغيير اللغة، الوضع الليلي، تفعيل إشعارات البريد).
- **Onboarding Tour Guides**: أدلة تعريفية تفاعلية تظهر للمستخدم عند تسجيل دخوله لأول مرة.

### 📝 دورة حياة البحث (للباحث)
- **Correction Submission Interface**: واجهة مخصصة للباحث لرفع التعديلات إذا تم رفض البحث (Non-Compliant Returned).
- **Researcher Personal History View**: سجل شخصي للباحث يرى فيه جميع أبحاثه السابقة ودرجاتها.

### ⚖️ لوحات التقييم واللجنة
- **Committee Member Workspace**: مساحة العمل الخاصة بالمقيم (حيث يقوم بإدخال الدرجات وكتابة التقرير الفني للبحث).
- **Member/Supervisor Personal History View**: سجل تاريخي لرئيس أو عضو اللجنة (الأبحاث التي قيمها سابقاً).
- **Evaluator Historic Portfolio View**: بورتفوليو المقيم (إحصائيات حول أداء المقيم وسرعته في التقييم).

### 🛡️ الحوكمة وإدارة النظام
- **SLA 10-Days Violation Grid**: شبكة لمراقبة الأبحاث المتأخرة لدى المقيمين (التي تجاوزت 10 أيام) لاتخاذ إجراءات.
- **Plagiarism Lockout Override Console**: لوحة لرئيس اللجنة لرفع عقوبة حظر الانتحال (3 أشهر) مع إرفاق المبرر.
- **HR Metadata Dictionary Manager**: شاشة لإدارة القوائم المنسدلة (المديريات، الأقسام، التخصصات).
- **System Notifications Audit Log**: سجل البنية التحتية لتتبع وصول الإشعارات ورسائل البريد الإلكتروني.

### 📅 إدارة الاجتماعات
- **Meeting Scheduler & Agenda Builder**: شاشة للسكرتير لجدولة اجتماع جديد، رفع جدول الأعمال، واختيار الحضور.
- **RSVP Real-Time Monitor**: شاشة لرئيس اللجنة لمتابعة من وافق/رفض الحضور للاجتماع في الوقت الفعلي.
- **Evaluator Anonymous Helpdesk Chat**: غرفة دردشة معزولة بين المقيمين الخارجيين والدعم الفني (بدون كشف هويتهم للباحثين).

---

## 🟡 ثالثاً: شاشات "غير مذكورة" في الـ Blueprint ولكن أرى أنها ضرورية جداً (Must-Have)

1. **Global Search & Filter Hub (محرك بحث متقدم):**
   - شاشة تتيح للسكرتير أو رئيس اللجنة البحث عن أي بحث بناءً على (Tracking Number، اسم الباحث، التخصص، أو الكلمات المفتاحية في الـ Abstract). النظام يحتوي على Full-Text Indexing، لذلك يجب استغلاله بواجهة قوية.

2. **Secure File Viewer (مستعرض الملفات الآمن):**
   - بما أن الملفات حساسة وتستخدم `FileAccessTokens` عبر الـ FTP، يجب ألا يتم تنزيل الملفات مباشرة بشكل يسهل تسريبها. نحتاج إلى Component داخلي (PDF Viewer) يعرض البحث داخل النظام (Stream) مع وضع علامة مائية (Watermark) باسم المستخدم الحالي.

3. **Delegation Management Portal (بوابة تفويض الصلاحيات):**
   - الـ Blueprint يذكر جدول `DelegatedRoles` (تفويض مؤقت للصلاحيات، مثلاً سكرتير يفوض موظف آخر أثناء إجازته). نحتاج شاشة للمستخدمين لإدارة هذه التفويضات (تاريخ البدء، الانتهاء، وإلغاء التفويض).

4. **Error & Access Denied Pages (صفحات الخطأ 403 / 404):**
   - صفحات مصممة بنمط النظام تظهر عندما يحاول مستخدم (مثلاً باحث) الدخول إلى رابط مخصص للمشرفين، أو عند طلب بحث غير موجود.

5. **Export & Reporting Hub (مركز تصدير التقارير):**
   - شاشة مخصصة لتصدير بيانات النظام بصيغة Excel (ClosedXML) أو PDF، مثل (تقرير الأبحاث المنجزة هذا الشهر، تقرير تقييم أداء المقيمين، إلخ).

6. **Landing / Home Dashboard (لوحة تحكم عامة):**
   - الـ `analytics-dashboard` مخصصة للإدارة العليا. لكن ماذا يرى الباحث أو المقيم فور دخوله؟ يجب أن تكون هناك لوحة تحكم بسيطة (Landing Page) تحتوي على (المهام المطلوبة منه، آخر الإشعارات، والأبحاث التي تنتظر تدخله).
