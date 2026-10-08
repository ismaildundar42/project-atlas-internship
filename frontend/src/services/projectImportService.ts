import apiClient from './apiClient';
import type {
  InspectWorkbookResponseDto,
  ValidateImportRequestDto,
  ValidateImportResponseDto,
  ConfirmImportRequestDto,
  ConfirmImportResponseDto,
} from '../types/importExport';

export const projectImportService = {
  /**
   * Resmi Excel şablon dosyasını indirir.
   */
  async downloadTemplate(): Promise<void> {
    const response = await apiClient.get<Blob>('/api/admin/projects/import/template', {
      responseType: 'blob',
    });

    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'DemirExport_Proje_Ice_Aktarim_Sablonu.xlsx');
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },

  /**
   * Excel dosyasını inceler, sayfaları, başlıkları ve eşleme önerilerini döndürür.
   */
  async inspectWorkbook(file: File): Promise<InspectWorkbookResponseDto> {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<InspectWorkbookResponseDto>(
      '/api/admin/projects/import/inspect',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );

    return response.data;
  },

  /**
   * Eşlenen sütunlara göre çalışma kitabını doğrular ve önizleme verilerini döndürür.
   */
  async validateImport(request: ValidateImportRequestDto): Promise<ValidateImportResponseDto> {
    const response = await apiClient.post<ValidateImportResponseDto>(
      '/api/admin/projects/import/validate',
      request
    );

    return response.data;
  },

  /**
   * Doğrulanmış projeleri veritabanına atomik olarak taslak statüsünde aktarır.
   */
  async confirmImport(request: ConfirmImportRequestDto): Promise<ConfirmImportResponseDto> {
    const response = await apiClient.post<ConfirmImportResponseDto>(
      '/api/admin/projects/import/confirm',
      request
    );

    return response.data;
  },

  /**
   * Filtrelenmiş aktif proje listesini Excel dosyası olarak indirir.
   */
  async exportProjects(params?: {
    search?: string;
    statusId?: number;
    publicationState?: string;
    approvalState?: string;
    lifecycleState?: string;
    sortBy?: string;
    sortDirection?: string;
  }): Promise<void> {
    const response = await apiClient.get<Blob>('/api/admin/projects/export', {
      params,
      responseType: 'blob',
    });

    let filename = `DemirExport_Projeler_${new Date().toISOString().split('T')[0]}.xlsx`;
    const disposition = response.headers?.['content-disposition'] || response.headers?.['Content-Disposition'];
    if (disposition && typeof disposition === 'string') {
      const match = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/i);
      if (match && match[1]) {
        filename = match[1].replace(/['"]/g, '').trim();
      }
    }

    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(url);
  },
};
