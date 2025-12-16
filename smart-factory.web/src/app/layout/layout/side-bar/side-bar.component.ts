import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface MenuItem {
  label: string;
  icon: string;
  route?: string;
  badge?: number;
  active?: boolean;
}

interface ProjectItem {
  label: string;
  route: string;
  color: string;
  active?: boolean;
}

@Component({
  selector: 'app-side-bar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.scss'
})
export class SideBarComponent implements OnInit {
  currentWorkspace: string = 'Keitoto Studio';
  showFavorites: boolean = false;

  // User info
  userName: string = '';
  userEmail: string = '';
  userAvatar: string = '';
  userInitials: string = '';

  mainMenuItems: MenuItem[] = [
    { label: 'Trang chủ', icon: 'pi-home', route: '/dashboard', active: true },
  ];

  favoriteItems: MenuItem[] = [];

  systemItems: ProjectItem[] = [
    { label: 'Người dùng', route: '/system/users', color: '#6366F1', active: true },
    { label: 'Thông tin công ty', route: '/system/unit-info', color: '#8B5CF6' },
  ];

  constructor() {}

  ngOnInit(): void {
    this.loadUserInfo();
  }

  loadUserInfo(): void {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        this.userName = user.fullName || user.userName || 'User';
        this.userEmail = user.email || '';
        this.userAvatar = user.avatar || '';
        this.userInitials = this.getInitials(this.userName);
      } catch {
        this.userName = 'User';
        this.userInitials = 'U';
      }
    } else {
      this.userName = 'Guest';
      this.userInitials = 'G';
    }
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }

  toggleSection(section: string): void {
  }
}
