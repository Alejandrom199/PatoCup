import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TournamentBracket } from './tournament-bracket';

describe('TournamentBracket', () => {
  let component: TournamentBracket;
  let fixture: ComponentFixture<TournamentBracket>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TournamentBracket]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TournamentBracket);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
