import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import { jobApplicationsApi } from '../../services/api';

export enum ApplicationStatus {
  Applied = 'Applied',
  Interviewing = 'Interviewing',
  Offer = 'Offer',
  Rejected = 'Rejected',
  Accepted = 'Accepted',
  Withdrawn = 'Withdrawn'
}

export interface JobApplication {
  id: number;
  jobTitle: string;
  companyId: number;
  companyName: string;
  location?: string;
  jobUrl?: string;
  status: ApplicationStatus;
  dateApplied: string;
  source?: string;
  tags: string[];
  contactPersonName?: string;
  contactPersonEmail?: string;
  contactPersonPhone?: string;
  notes?: string;
  resumeId?: number;
  resumeTitle?: string;
  attachmentPaths: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateJobApplicationDto {
  jobTitle: string;
  companyId: number;
  location?: string;
  jobUrl?: string;
  status?: ApplicationStatus;
  dateApplied: string; // Required string in YYYY-MM-DD format
  source?: string;
  tags?: string[];
  contactPersonName?: string;
  contactPersonEmail?: string;
  contactPersonPhone?: string;
  notes?: string;
  resumeId?: number;
  attachmentPaths?: string[];
}

interface JobApplicationsState {
  applications: JobApplication[];
  currentApplication: JobApplication | null;
  dashboard: any | null;
  isLoading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
  filters: {
    status?: ApplicationStatus;
    search?: string;
    tags?: string;
  };
}

const initialState: JobApplicationsState = {
  applications: [],
  currentApplication: null,
  dashboard: null,
  isLoading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20,
  filters: {},
};

// Async thunks
export const fetchJobApplications = createAsyncThunk(
  'jobApplications/fetchJobApplications',
  async (params: {
    page?: number;
    pageSize?: number;
    status?: ApplicationStatus;
    search?: string;
    tags?: string;
  } = {}, { rejectWithValue }) => {
    try {
      const response = await jobApplicationsApi.getJobApplications(params);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch job applications');
    }
  }
);

export const fetchJobApplication = createAsyncThunk(
  'jobApplications/fetchJobApplication',
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await jobApplicationsApi.getJobApplication(id);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch job application');
    }
  }
);

export const createJobApplication = createAsyncThunk(
  'jobApplications/createJobApplication',
  async (data: CreateJobApplicationDto, { rejectWithValue }) => {
    try {
      const response = await jobApplicationsApi.createJobApplication(data);
      return response;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to create job application');
    }
  }
);

export const updateJobApplication = createAsyncThunk(
  'jobApplications/updateJobApplication',
  async ({ id, data }: { id: number; data: Partial<CreateJobApplicationDto> }, { rejectWithValue }) => {
    try {
      await jobApplicationsApi.updateJobApplication(id, data);
      return { id, data };
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to update job application');
    }
  }
);

export const deleteJobApplication = createAsyncThunk(
  'jobApplications/deleteJobApplication',
  async (id: number, { rejectWithValue }) => {
    try {
      await jobApplicationsApi.deleteJobApplication(id);
      return id;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to delete job application');
    }
  }
);

export const getDashboard = createAsyncThunk(
  'jobApplications/getDashboard',
  async (_, { rejectWithValue }) => {
    try {
      const response = await jobApplicationsApi.getDashboard();
      return response;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch dashboard data');
    }
  }
);

const jobApplicationsSlice = createSlice({
  name: 'jobApplications',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
    setFilters: (state, action: PayloadAction<Partial<JobApplicationsState['filters']>>) => {
      state.filters = { ...state.filters, ...action.payload };
    },
    clearFilters: (state) => {
      state.filters = {};
    },
    setCurrentPage: (state, action: PayloadAction<number>) => {
      state.currentPage = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch job applications
      .addCase(fetchJobApplications.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchJobApplications.fulfilled, (state, action) => {
        state.isLoading = false;
        state.applications = action.payload.data;
        state.totalCount = action.payload.totalCount;
        state.currentPage = action.payload.page;
        state.pageSize = action.payload.pageSize;
      })
      .addCase(fetchJobApplications.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch single job application
      .addCase(fetchJobApplication.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchJobApplication.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentApplication = action.payload;
      })
      .addCase(fetchJobApplication.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Create job application
      .addCase(createJobApplication.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(createJobApplication.fulfilled, (state, action) => {
        state.isLoading = false;
        state.applications.unshift(action.payload);
        state.totalCount += 1;
      })
      .addCase(createJobApplication.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Update job application
      .addCase(updateJobApplication.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(updateJobApplication.fulfilled, (state, action) => {
        state.isLoading = false;
        const index = state.applications.findIndex(app => app.id === action.payload.id);
        if (index !== -1) {
          state.applications[index] = { ...state.applications[index], ...action.payload.data };
        }
        if (state.currentApplication && state.currentApplication.id === action.payload.id) {
          state.currentApplication = { ...state.currentApplication, ...action.payload.data };
        }
      })
      .addCase(updateJobApplication.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Delete job application
      .addCase(deleteJobApplication.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(deleteJobApplication.fulfilled, (state, action) => {
        state.isLoading = false;
        state.applications = state.applications.filter(app => app.id !== action.payload);
        state.totalCount -= 1;
        if (state.currentApplication && state.currentApplication.id === action.payload) {
          state.currentApplication = null;
        }
      })
      .addCase(deleteJobApplication.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Get dashboard
      .addCase(getDashboard.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(getDashboard.fulfilled, (state, action) => {
        state.isLoading = false;
        state.dashboard = action.payload;
      })
      .addCase(getDashboard.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      });
  },
});

export const { clearError, setFilters, clearFilters, setCurrentPage } = jobApplicationsSlice.actions;
export default jobApplicationsSlice.reducer;
