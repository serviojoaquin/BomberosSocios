import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map, switchMap } from 'rxjs/operators';
import { CurrentUserVm, LoginRequest, RegisterRequest, UsersClient } from '../app/web-api-client';
import { Permissions, hasPermission } from '../app/permissions';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _currentUser = new BehaviorSubject<CurrentUserVm | null>(null);
  currentUser$ = this._currentUser.asObservable();
  isAuthenticated$ = this.currentUser$.pipe(map(user => user !== null));

  constructor(private usersClient: UsersClient) {}

  initialize(): Observable<boolean> {
    return this.loadCurrentUser().pipe(map(user => user !== null));
  }

  login(email: string, password: string): Observable<void> {
    return this.usersClient.login(true, undefined, new LoginRequest({ email, password })).pipe(
      switchMap(() => this.loadCurrentUser()),
      map(() => void 0)
    );
  }

  register(email: string, password: string): Observable<void> {
    return this.usersClient.register(new RegisterRequest({ email, password }));
  }

  logout(): Observable<void> {
    return this.usersClient.logout({}).pipe(
      tap(() => this._currentUser.next(null))
    );
  }

  hasPermission(required: Permissions): boolean {
    const user = this._currentUser.value;
    return user !== null && hasPermission(user.permissions, required);
  }

  hasPermission$(required: Permissions): Observable<boolean> {
    return this.currentUser$.pipe(
      map(user => user !== null && hasPermission(user.permissions, required))
    );
  }

  private loadCurrentUser(): Observable<CurrentUserVm | null> {
    return this.usersClient.getCurrentUser().pipe(
      catchError(() => of(null)),
      tap(user => this._currentUser.next(user))
    );
  }
}
