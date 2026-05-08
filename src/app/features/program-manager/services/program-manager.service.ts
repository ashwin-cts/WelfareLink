import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_CONFIG } from '../../../core/config/api.config';
import { WelfareProgram, Resource, BudgetMonitoring } from '../models/program.model';

@Injectable({
  providedIn: 'root'
})
export class ProgramManagerService {
  private http = inject(HttpClient);
  private config = inject(API_CONFIG);

  // --- Welfare Program Endpoints ---

  getPrograms(): Observable<WelfareProgram[]> {
    return this.http.get<WelfareProgram[]>(this.config.programApi);
  }

  getProgramById(id: number): Observable<WelfareProgram> {
    return this.http.get<WelfareProgram>(`${this.config.programApi}/${id}`);
  }

  createProgram(program: WelfareProgram): Observable<WelfareProgram> {
    return this.http.post<WelfareProgram>(this.config.programApi, program);
  }

  updateProgram(id: number, program: WelfareProgram): Observable<void> {
    return this.http.put<void>(`${this.config.programApi}/${id}`, program);
  }

  suspendProgram(id: number): Observable<void> {
    return this.http.patch<void>(`${this.config.programApi}/${id}/suspend`, {});
  }

  getBudgetMonitoring(): Observable<BudgetMonitoring[]> {
    return this.http.get<BudgetMonitoring[]>(`${this.config.programApi}/budget-monitoring`);
  }

  // --- Resource Endpoints ---

  getResources(): Observable<Resource[]> {
    return this.http.get<Resource[]>(this.config.resourceApi);
  }

  getResourcesByProgram(programId: number): Observable<Resource[]> {
    return this.http.get<Resource[]>(`${this.config.resourceApi}/program/${programId}`);
  }

  addResource(resource: Resource): Observable<Resource> {
    return this.http.post<Resource>(this.config.resourceApi, resource);
  }

  updateResource(id: number, resource: Resource): Observable<void> {
    return this.http.put<void>(`${this.config.resourceApi}/${id}`, resource);
  }

  getResourceUtilisation(): Observable<any> {
    return this.http.get(`${this.config.resourceApi}/utilisation`);
  }
}