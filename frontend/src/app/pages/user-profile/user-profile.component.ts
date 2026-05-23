import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.scss']
})
export class UserProfileComponent implements OnInit {
  authService = inject(AuthService);
  toastService = inject(ToastService);

  user: any = null;
  role: string = '';
  
  // Preferences State
  isDarkMode: boolean = false;
  emailNotifications: boolean = true;
  language: string = 'ar';

  ngOnInit() {
    this.user = this.authService.currentUser();
    this.role = this.authService.getRole();
    
    // Load preferences from localStorage as a mockup for UserSettings table
    const storedTheme = localStorage.getItem('boc_theme');
    if (storedTheme === 'dark') {
      this.isDarkMode = true;
      document.body.classList.add('dark-mode');
    }
  }

  toggleTheme() {
    this.isDarkMode = !this.isDarkMode;
    if (this.isDarkMode) {
      document.body.classList.add('dark-mode');
      localStorage.setItem('boc_theme', 'dark');
    } else {
      document.body.classList.remove('dark-mode');
      localStorage.setItem('boc_theme', 'light');
    }
    this.toastService.success('تم تحديث السمة (Theme) بنجاح.');
  }

  savePreferences() {
    this.toastService.success('تم حفظ التفضيلات الشخصية بنجاح.');
  }
}
