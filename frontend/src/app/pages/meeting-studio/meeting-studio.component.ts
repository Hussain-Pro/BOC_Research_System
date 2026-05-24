import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MeetingService, MeetingDetails, MeetingPaper } from '../../services/meeting.service';
import { SignalRService, ChatMessage } from '../../services/signalr.service';
import { AuthService } from '../../services/auth.service';
import { BocLayoutService } from '../../services/boc-layout.service';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-meeting-studio',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './meeting-studio.component.html',
  styleUrl: './meeting-studio.component.scss'
})
export class MeetingStudioComponent implements OnInit, OnDestroy {
  private layoutService = inject(BocLayoutService);

  meetingId = signal<string>('6B2A9B10-8D7C-6B5A-4928-1029384756AF');
  meetingDetails = signal<MeetingDetails | null>(null);
  activePaper = signal<MeetingPaper | null>(null);

  chatMessages: ChatMessage[] = [];
  newMessage = '';
  committeeChannelId = '12345678-1234-1234-1234-123456789abc';

  section1 = 'افتتاح الجلسة وإعلان الحضور: عقدت لجنة دراسة وتقييم البحوث برئاسة السيد رئيس اللجنة، وبحضور السادة أعضاء اللجنة الموقرين للتباحث في البحوث المرفوعة.';
  section2 = 'مراجعة البحوث وخلاصة تقارير المقيمين: تم استعراض تقارير المقيمين الخارجيين الخاصة بالبحوث العلمية المطروحة ودرجات التقييم.';
  section3 = 'محضر مناقشات ومداولات اللجنة: جرت مناقشات مستفيضة بين أعضاء اللجنة حول الجدوى الفنية والتطبيقية للبحوث داخل شركة نفط البصرة.';
  section4 = 'نتائج التصويت وتقييم رئيس اللجنة: جرى التصويت على البحوث بشكل قانوني وسجلت درجات رئيس اللجنة.';
  section5 = 'القرار النهائي والتوقيعات: بناءً على ما تقدم، نوصي بالموافقة على البحوث المستوفية وإرسالها لمديرية الموارد البشرية.';

  compiledContent = '';

  chairmanScore = 0;
  chairmanComments = '';

  votingMemberId = '';
  votingValue = 'Approve';

  success = signal<boolean>(false);
  error = signal<string | null>(null);
  infoMessage = signal<string | null>(null);
  isLoading = signal<boolean>(false);

  breadcrumbs = [
    { label: 'الرئيسية', route: '/home' },
    { label: 'استوديو محاضر الاجتماعات' }
  ];

  private subscriptions = new Subscription();

  constructor(
    private route: ActivatedRoute,
    private meetingService: MeetingService,
    private signalRService: SignalRService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.layoutService.setPage('استوديو محاضر الاجتماعات');
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.meetingId.set(idParam);
    }

    this.loadMeetingDetails();

    const token = this.authService.token();
    if (token) {
      this.signalRService.startConnections(token);
      setTimeout(() => {
        this.signalRService.joinChatChannel(this.committeeChannelId);
      }, 1000);
    }

    this.subscriptions.add(
      this.signalRService.chatMessages$.subscribe((msg) => {
        this.chatMessages.push(msg);
      })
    );

    this.subscriptions.add(
      this.signalRService.notifications$.subscribe((notif) => {
        this.infoMessage.set(`إشعار عاجل: ${notif.title} - ${notif.message}`);
        setTimeout(() => this.infoMessage.set(null), 5000);
      })
    );
  }

  ngOnDestroy() {
    this.signalRService.leaveChatChannel(this.committeeChannelId);
    this.signalRService.stopConnections();
    this.subscriptions.unsubscribe();
  }

  loadMeetingDetails() {
    this.isLoading.set(true);
    this.meetingService.getMeetingDetails(this.meetingId()).subscribe({
      next: (details) => {
        this.meetingDetails.set(details);
        this.isLoading.set(false);
        this.prepopulateCompiler(details);
      },
      error: () => {
        this.error.set('فشل تحميل تفاصيل الاجتماع والمحاضر.');
        this.isLoading.set(false);
      }
    });
  }

  prepopulateCompiler(details: MeetingDetails) {
    this.section1 = `افتتاح الجلسة وإعلان الحضور: عقدت لجنة دراسة وتقييم البحوث برئاسة السيد رئيس اللجنة، وبحضور السادة أعضاء اللجنة الموقرين للتباحث في اجتماع اللجنة رقم (${details.meetingNumber}) في موقع (${details.location}) بتاريخ (${new Date(details.scheduledDate).toLocaleDateString('ar-EG')}).`;
    this.compileMinutes();
  }

  compileMinutes() {
    this.compiledContent = `${this.section1}\n\n${this.section2}\n\n${this.section3}\n\n${this.section4}\n\n${this.section5}`;
  }

  selectPaper(paper: MeetingPaper) {
    this.activePaper.set(paper);
    this.chairmanScore = paper.chairmanScore || 0;
    this.chairmanComments = '';
    this.error.set(null);
    this.success.set(false);
  }

  onCastVote() {
    const paper = this.activePaper();
    const details = this.meetingDetails();
    if (!paper || !details) return;

    if (!this.votingMemberId) {
      this.error.set('يرجى اختيار عضو اللجنة للتصويت.');
      return;
    }

    this.error.set(null);
    this.success.set(false);

    this.meetingService.castVote(details.id, paper.id, this.votingMemberId, this.votingValue).subscribe({
      next: () => {
        this.success.set(true);
        this.loadMeetingDetails();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'فشل تسجيل التصويت.');
      }
    });
  }

  onSubmitGrade() {
    const paper = this.activePaper();
    const details = this.meetingDetails();
    if (!paper || !details) return;

    if (!details.minutesId) {
      this.error.set('لا يمكن تسجيل الدرجة بدون محضر اجتماع مسبق.');
      return;
    }

    this.error.set(null);
    this.success.set(false);

    const currentUser = this.authService.currentUser();
    const chairmanId = currentUser?.nameid || 'D2A9B10E-8D7C-6B5A-4928-1029384756AE';

    this.meetingService.submitGrade(paper.id, chairmanId, details.minutesId, this.chairmanScore, this.chairmanComments).subscribe({
      next: () => {
        this.success.set(true);
        this.loadMeetingDetails();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'فشل حفظ درجة رئيس اللجنة.');
      }
    });
  }

  onFreezeMinutes() {
    const details = this.meetingDetails();
    if (!details || !details.minutesId) return;

    if (!confirm('هل أنت متأكد من تجميد المحضر؟ هذه العملية نهائية وتمنع إجراء أي تعديل أو تصويت لاحقاً.')) {
      return;
    }

    this.error.set(null);
    this.success.set(false);

    const currentUser = this.authService.currentUser();
    const frozenById = currentUser?.nameid || 'D2A9B10E-8D7C-6B5A-4928-1029384756AE';

    this.meetingService.freezeMinutes(details.minutesId, frozenById).subscribe({
      next: () => {
        this.success.set(true);
        this.loadMeetingDetails();
      },
      error: (err) => {
        this.error.set(err.error?.detail || 'فشل تجميد محضر الاجتماع.');
      }
    });
  }

  sendChatMessage() {
    if (!this.newMessage.trim()) return;

    this.signalRService.sendMessage(this.committeeChannelId, null, this.newMessage)
      .then(() => {
        this.newMessage = '';
      })
      .catch(() => {
        this.error.set('خطأ أثناء إرسال الرسالة إلى Hub.');
      });
  }

  getVoteCount(paperId: string, value: string): number {
    return this.meetingDetails()?.votes.filter(v => v.researchId === paperId && v.voteValue === value).length || 0;
  }

  get minutesStatusLabel(): string {
    const status = this.meetingDetails()?.minutesStatus;
    if (status === 'Minutes_Frozen') return 'مجمد ومغلق';
    if (status === 'Draft') return 'مسودة نشطة';
    return 'لا يوجد محضر';
  }

  get attendedCount(): number {
    return this.meetingDetails()?.attendees?.filter(a => a.attended).length || 0;
  }
}
