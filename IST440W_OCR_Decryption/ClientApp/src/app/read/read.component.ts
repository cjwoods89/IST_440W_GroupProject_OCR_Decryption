import { Component, OnInit } from '@angular/core';
import { ComputervisionService } from '../services/computervision.service';
import { AvailableLanguage } from '../models/availablelanguage';
import { ReadResult } from '../models/readresult';

@Component({
    selector: 'app-ocr',
    templateUrl: './read.component.html',
    styleUrls: ['./read.component.css']
  })

  export class ReadComponent implements OnInit {
    loading = false;
    imageFile;
    imagePreview;
    imageData = new FormData();
    availableLanguage: AvailableLanguage[];
    DetectedTextLanguage: string;
    readResult: ReadResult;
    DefaultStatus: string;
    status: string;
    maxFileSize: number;
    isValidFile = true;

    constructor(private computervisionService: ComputervisionService) {
        this.DefaultStatus = "Maximum size allowed for the image is 4 MB";
        this.status = this.DefaultStatus;
        this.maxFileSize = 4 * 1024 * 1024; // 4MB
    }
    
    ngOnInit() {
        this.computervisionService.getAvailableLanguage().subscribe(
            (result: AvailableLanguage[]) => this.availableLanguage = result
        );
    }

    uploadImage(event) {
        this.imageFile = event.target.files[0];
        if (this.imageFile.size > this.maxFileSize) {
            this.status = `The file size is ${this.imageFile.size} bytes, this is more than the allowed limit of ${this.maxFileSize} bytes.`;
            this.isValidFile = false;
        } else if (this.imageFile.type.indexOf('image') == -1) {
            this.status = "Please upload a valid image file";
            this.isValidFile = false;
        } else {
            const reader = new FileReader();
            reader.readAsDataURL(event.target.files[0]);
            reader.onload = () => {
            this.imagePreview = reader.result;
            };
            this.status = this.DefaultStatus;
            this.isValidFile = true;
        }
    }
    
    GetText() {
        if (this.isValidFile) {
    
          this.loading = true;
          this.imageData.append('imageFile', this.imageFile);
    
          this.computervisionService.getTextFromWrittenImage(this.imageData).subscribe(
            (result: ReadResult) => {
              this.readResult = result;
              if (this.availableLanguage.find(x => x.languageID === this.readResult.language)) {
                this.DetectedTextLanguage = this.availableLanguage.find(x => x.languageID === this.readResult.language).languageName;
              } else {
                this.DetectedTextLanguage = "unknown";
              }
              this.loading = false;
            });
        }
    }   
}