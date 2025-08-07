import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { resumesApi } from '../../services/api';

export interface Resume {
  id: number;
  title: string;
  description?: string;
  filePath: string;
  fileType: string;
  fileSizeBytes: number;
  tags: string[];
  isDefault: boolean;
  createdAt: string;
  updatedAt: string;
  usageCount: number;
}

export interface CreateResumeDto {
  title: string;
  description?: string;
  tags?: string[];
  isDefault?: boolean;
}

interface ResumesState {
  resumes: Resume[];
  currentResume: Resume | null;
  isLoading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

const initialState: ResumesState = {
  resumes: [],
  currentResume: null,
  isLoading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20,
};

// Async thunks
export const fetchResumes = createAsyncThunk(
  'resumes/fetchResumes',
  async (params: {
    page?: number;
    pageSize?: number;
    search?: string;
  } = {}, { rejectWithValue }) => {
    try {
      const response = await resumesApi.getResumes(params);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch resumes');
      }
      return rejectWithValue('Failed to fetch resumes');
    }
  }
);

export const fetchResume = createAsyncThunk(
  'resumes/fetchResume',
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await resumesApi.getResume(id);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch resume');
      }
      return rejectWithValue('Failed to fetch resume');
    }
  }
);

export const createResume = createAsyncThunk(
  'resumes/createResume',
  async ({ data, file }: { data: CreateResumeDto; file: File }, { rejectWithValue }) => {
    try {
      const response = await resumesApi.createResume(data, file);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to create resume');
      }
      return rejectWithValue('Failed to create resume');
    }
  }
);

export const updateResume = createAsyncThunk(
  'resumes/updateResume',
  async ({ id, data }: { id: number; data: Partial<CreateResumeDto> }, { rejectWithValue }) => {
    try {
      await resumesApi.updateResume(id, data);
      return { id, data };
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to update resume');
      }
      return rejectWithValue('Failed to update resume');
    }
  }
);

export const deleteResume = createAsyncThunk(
  'resumes/deleteResume',
  async (id: number, { rejectWithValue }) => {
    try {
      await resumesApi.deleteResume(id);
      return id;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to delete resume');
      }
      return rejectWithValue('Failed to delete resume');
    }
  }
);

const resumesSlice = createSlice({
  name: 'resumes',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch resumes
      .addCase(fetchResumes.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchResumes.fulfilled, (state, action) => {
        state.isLoading = false;
        state.resumes = action.payload.data;
        state.totalCount = action.payload.totalCount;
        state.currentPage = action.payload.page;
        state.pageSize = action.payload.pageSize;
      })
      .addCase(fetchResumes.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch single resume
      .addCase(fetchResume.fulfilled, (state, action) => {
        state.currentResume = action.payload;
      })
      // Create resume
      .addCase(createResume.fulfilled, (state, action) => {
        state.resumes.unshift(action.payload);
        state.totalCount += 1;
      })
      // Update resume
      .addCase(updateResume.fulfilled, (state, action) => {
        const index = state.resumes.findIndex(resume => resume.id === action.payload.id);
        if (index !== -1) {
          state.resumes[index] = { ...state.resumes[index], ...action.payload.data };
        }
      })
      // Delete resume
      .addCase(deleteResume.fulfilled, (state, action) => {
        state.resumes = state.resumes.filter(resume => resume.id !== action.payload);
        state.totalCount -= 1;
      });
  },
});

export const { clearError } = resumesSlice.actions;
export default resumesSlice.reducer;
