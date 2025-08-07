import axios from 'axios';
import Cookies from 'js-cookie';
import type { ApplicationStatus, JobApplication, CreateJobApplicationDto } from '../store/slices/jobApplicationsSlice';
import type { Company, CreateCompanyDto } from '../store/slices/companiesSlice';
import type { Resume, CreateResumeDto } from '../store/slices/resumesSlice';
import type { Feedback, CreateFeedbackDto } from '../store/slices/feedbackSlice';

// Create axios instance
const api = axios.create({
  baseURL: 'http://localhost:5213/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add auth token
api.interceptors.request.use(
  (config) => {
    console.log("Request config:", config);
    const token = Cookies.get('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle auth errors
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.log("Response error:", error);
    if (error.response?.status === 401) {
      Cookies.remove('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth API
export const authApi = {
  login: async (credentials: { email: string; password: string }) => {
    console.log("Login credentials:", credentials);
    const response = await api.post('/auth/login', credentials);
    console.log("Login response:", response.data);
    return response.data;
  },

  register: async (userData: { 
    email: string; 
    password: string; 
    confirmPassword: string;
    firstName: string; 
    lastName: string; 
  }) => {
    try {
      const response = await api.post('/auth/register', userData);
      return response.data;
    } catch (error) {
      console.error('Registration error:', error);
      if (axios.isAxiosError(error)) {
        if (error.response) {
          console.error('Error response:', error.response.data);
          console.error('Status code:', error.response.status);
          console.error('Headers:', error.response.headers);
        } else if (error.request) {
          console.error('Error request:', error.request);
        }
      } else {
        console.error('Error message:', (error as Error).message);
      }
      throw error;
    }
  },

  getCurrentUser: async () => {
    const response = await api.get('/auth/me');
    return response.data;
  },
};

// Job Applications API
export const jobApplicationsApi = {
  getJobApplications: async (params: {
    page?: number;
    pageSize?: number;
    status?: ApplicationStatus;
    search?: string;
    tags?: string;
  } = {}) => {
    const response = await api.get('/jobapplications', { params });
    return {
      data: response.data,
      totalCount: parseInt(response.headers['x-total-count'] || '0'),
      page: parseInt(response.headers['x-page'] || '1'),
      pageSize: parseInt(response.headers['x-page-size'] || '20'),
    };
  },

  getJobApplication: async (id: number): Promise<JobApplication> => {
    const response = await api.get(`/jobapplications/${id}`);
    return response.data;
  },

  createJobApplication: async (data: CreateJobApplicationDto): Promise<JobApplication> => {
    const response = await api.post('/jobapplications', data);
    return response.data;
  },

  updateJobApplication: async (id: number, data: Partial<CreateJobApplicationDto>) => {
    const response = await api.put(`/jobapplications/${id}`, data);
    return response.data;
  },

  deleteJobApplication: async (id: number) => {
    await api.delete(`/jobapplications/${id}`);
  },

  getDashboard: async () => {
    const response = await api.get('/jobapplications/dashboard');
    return response.data;
  },
};

// Companies API
export const companiesApi = {
  getCompanies: async (params: {
    page?: number;
    pageSize?: number;
    search?: string;
  } = {}) => {
    const response = await api.get('/companies', { params });
    return {
      data: response.data,
      totalCount: parseInt(response.headers['x-total-count'] || '0'),
      page: parseInt(response.headers['x-page'] || '1'),
      pageSize: parseInt(response.headers['x-page-size'] || '20'),
    };
  },

  getCompany: async (id: number): Promise<Company> => {
    const response = await api.get(`/companies/${id}`);
    return response.data;
  },

  createCompany: async (data: CreateCompanyDto): Promise<Company> => {
    const response = await api.post('/companies', data);
    return response.data;
  },

  updateCompany: async (id: number, data: Partial<CreateCompanyDto>) => {
    await api.put(`/companies/${id}`, data);
  },

  deleteCompany: async (id: number) => {
    await api.delete(`/companies/${id}`);
  },

  getCompanyContacts: async (companyId: number) => {
    const response = await api.get(`/companies/${companyId}/contacts`);
    return response.data;
  },

  createCompanyContact: async (companyId: number, data: {
    name: string;
    position?: string;
    email?: string;
    phone?: string;
    linkedin?: string;
    notes?: string;
  }) => {
    const response = await api.post(`/companies/${companyId}/contacts`, data);
    return response.data;
  },

  deleteCompanyContact: async (companyId: number, contactId: number) => {
    await api.delete(`/companies/${companyId}/contacts/${contactId}`);
  },
};

// Resumes API
export const resumesApi = {
  getResumes: async (params: {
    page?: number;
    pageSize?: number;
    search?: string;
  } = {}) => {
    const response = await api.get('/resumes', { params });
    return {
      data: response.data,
      totalCount: parseInt(response.headers['x-total-count'] || '0'),
      page: parseInt(response.headers['x-page'] || '1'),
      pageSize: parseInt(response.headers['x-page-size'] || '20'),
    };
  },

  getResume: async (id: number): Promise<Resume> => {
    const response = await api.get(`/resumes/${id}`);
    return response.data;
  },

  createResume: async (data: CreateResumeDto, file: File): Promise<Resume> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', data.title);
    if (data.description) formData.append('description', data.description);
    if (data.tags) formData.append('tags', JSON.stringify(data.tags));
    if (data.isDefault !== undefined) formData.append('isDefault', data.isDefault.toString());

    const response = await api.post('/resumes', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  updateResume: async (id: number, data: Partial<CreateResumeDto>) => {
    await api.put(`/resumes/${id}`, data);
  },

  deleteResume: async (id: number) => {
    await api.delete(`/resumes/${id}`);
  },

  downloadResume: async (id: number) => {
    const response = await api.get(`/resumes/${id}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },

  getResumeVersions: async (resumeId: number) => {
    const response = await api.get(`/resumes/${resumeId}/versions`);
    return response.data;
  },

  createResumeVersion: async (resumeId: number, data: {
    versionName: string;
    changes?: string;
    jobDescription?: string;
    aiPrompt?: string;
  }, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('versionName', data.versionName);
    if (data.changes) formData.append('changes', data.changes);
    if (data.jobDescription) formData.append('jobDescription', data.jobDescription);
    if (data.aiPrompt) formData.append('aiPrompt', data.aiPrompt);

    const response = await api.post(`/resumes/${resumeId}/versions`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  downloadResumeVersion: async (resumeId: number, versionId: number) => {
    const response = await api.get(`/resumes/${resumeId}/versions/${versionId}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },
};

// Feedback API
export const feedbackApi = {
  getFeedbackForApplication: async (jobApplicationId: number): Promise<Feedback[]> => {
    const response = await api.get(`/feedback/application/${jobApplicationId}`);
    return response.data;
  },

  getFeedback: async (id: number): Promise<Feedback> => {
    const response = await api.get(`/feedback/${id}`);
    return response.data;
  },

  createFeedback: async (data: CreateFeedbackDto): Promise<Feedback> => {
    const response = await api.post('/feedback', data);
    return response.data;
  },

  updateFeedback: async (id: number, data: Partial<CreateFeedbackDto>) => {
    await api.put(`/feedback/${id}`, data);
  },

  deleteFeedback: async (id: number) => {
    await api.delete(`/feedback/${id}`);
  },

  getPendingFollowUps: async (): Promise<Feedback[]> => {
    const response = await api.get('/feedback/follow-ups');
    return response.data;
  },

  completeFollowUp: async (id: number) => {
    await api.post(`/feedback/${id}/complete-followup`);
  },
};

export default api;
