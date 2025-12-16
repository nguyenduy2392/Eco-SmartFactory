import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-loading',
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.scss']
})
export class LoadingComponent implements OnInit {

  isShow = true;
  constructor(
    private router: Router
  ) { }

  ngOnInit(): void {
    let url = this.router.url;
    if(url.includes("nhan-vien")){
      this.isShow = false;
    }
  }

}
