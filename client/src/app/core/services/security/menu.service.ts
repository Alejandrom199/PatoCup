import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/src/environments/environment.development';
import { ApiResponse } from '../../models/api-response';
import { GroupedMenu, MenuResponseDto } from '../../../data/features/tournaments/dtos/security/menu.dto';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}Auth/menu`;

  // Mapeo: "Lo que viene del C#" : "Lo que entiende HeroIcons"
  private iconMap: Record<string, string> = {
    'security': 'heroLockClosed',
    'user-cog': 'heroUser',
    'lock': 'heroLockClosed',
    'sports_esports': 'heroTrophy',
    'users': 'heroUsers',
    'list': 'heroSquares2x2',
    'history': 'heroShieldCheck',
    'audit': 'heroShieldCheck'
  };

  getMyMenu() {
    return this.http.get<ApiResponse<MenuResponseDto[]>>(this.apiUrl).pipe(
      map(res => {
        if (!res.succeeded || !res.data) return [];
        return this.groupMenu(res.data);
      })
    );
  }

  private groupMenu(data: MenuResponseDto[]): GroupedMenu[] {
    const groups: GroupedMenu[] = [];

    data.forEach(item => {
      let group = groups.find(g => g.id === item.menuId);
      if (!group) {
        group = {
          id: item.menuId,
          name: item.menuName,
          icon: this.iconMap[item.menuIcon] || 'heroSquares2x2',
          order: item.menuOrder,
          options: []
        };
        groups.push(group);
      }
      group.options.push({
        name: item.optionName,
        route: item.optionRoute,
        icon: this.iconMap[item.optionIcon] || 'heroChevronDown'
      });
    });

    return groups.sort((a, b) => a.order - b.order);
  }
}