import { Component, OnInit } from '@angular/core';

import {AuthService} from '../services/auth.service';

@Component({
  selector: 'app-protected',
  templateUrl: './protected.component.html',
  styleUrls: ['./protected.component.css']
})
export class ProtectedComponent implements OnInit {
  profile: string;
  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.profile = this.authService.getClaims();
  }

}
