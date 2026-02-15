export interface MenuResponseDto {
  menuId: number;
  menuName: string;
  menuIcon: string;
  menuOrder: number;
  optionId: number;
  optionName: string;
  optionRoute: string;
  optionIcon: string;
}

export interface GroupedMenu {
  id: number;
  name: string;
  icon: string;
  order: number;
  options: {
    name: string;
    route: string;
    icon: string;
  }[];
}