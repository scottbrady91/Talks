import { Component, OnInit } from '@angular/core';
import { Http, RequestOptions, Headers, Response } from '@angular/http';

import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-call-api',
  templateUrl: './call-api.component.html',
  styleUrls: ['./call-api.component.css']
})
export class CallApiComponent implements OnInit {
  response: string;
  constructor(private http: Http, private authService: AuthService) { }  

  ngOnInit() {
    let headers = new Headers({'Authorization': this.authService.getAuthorizationHeaderValue()});
    let options = new RequestOptions({headers: headers});
    this.http.get("http://localhost:5001/api/values", options)
      .subscribe(response => this.response = response.text());
  }
}
