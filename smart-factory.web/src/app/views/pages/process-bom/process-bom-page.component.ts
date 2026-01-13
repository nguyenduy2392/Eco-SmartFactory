import { Component } from '@angular/core';
import { SharedModule } from '../../../shared.module';
import { PrimengModule } from '../../../primeng.module';
import { ProcessBOMComponent } from '../../../components/process-bom/process-bom.component';

@Component({
  selector: 'app-process-bom-page',
  standalone: true,
  imports: [SharedModule, PrimengModule, ProcessBOMComponent],
  templateUrl: './process-bom-page.component.html',
  styleUrls: ['./process-bom-page.component.scss']
})
export class ProcessBomPageComponent {
  
}
