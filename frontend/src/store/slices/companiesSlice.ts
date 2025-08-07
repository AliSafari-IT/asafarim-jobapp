import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { companiesApi } from '../../services/api';

export interface Company {
  id: number;
  name: string;
  location?: string;
  website?: string;
  industry?: string;
  size?: string;
  description?: string;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  jobApplicationsCount: number;
}

export interface CreateCompanyDto {
  name: string;
  location?: string;
  website?: string;
  industry?: string;
  size?: string;
  description?: string;
  notes?: string;
}

interface CompaniesState {
  companies: Company[];
  currentCompany: Company | null;
  isLoading: boolean;
  error: string | null;
  totalCount: number;
  currentPage: number;
  pageSize: number;
}

const initialState: CompaniesState = {
  companies: [],
  currentCompany: null,
  isLoading: false,
  error: null,
  totalCount: 0,
  currentPage: 1,
  pageSize: 20,
};

// Async thunks
export const fetchCompanies = createAsyncThunk(
  'companies/fetchCompanies',
  async (params: {
    page?: number;
    pageSize?: number;
    search?: string;
  } = {}, { rejectWithValue }) => {
    try {
      const response = await companiesApi.getCompanies(params);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch companies');
      }
      return rejectWithValue('Failed to fetch companies');
    }
  }
);

export const fetchCompany = createAsyncThunk(
  'companies/fetchCompany',
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await companiesApi.getCompany(id);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch company');
      }
      return rejectWithValue('Failed to fetch company');
    }
  }
);

export const createCompany = createAsyncThunk(
  'companies/createCompany',
  async (data: CreateCompanyDto, { rejectWithValue }) => {
    try {
      const response = await companiesApi.createCompany(data);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to create company');
      }
      return rejectWithValue('Failed to create company');
    }
  }
);

export const updateCompany = createAsyncThunk(
  'companies/updateCompany',
  async ({ id, data }: { id: number; data: Partial<CreateCompanyDto> }, { rejectWithValue }) => {
    try {
      await companiesApi.updateCompany(id, data);
      return { id, data };
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to update company');
      }
      return rejectWithValue('Failed to update company');
    }
  }
);

export const deleteCompany = createAsyncThunk(
  'companies/deleteCompany',
  async (id: number, { rejectWithValue }) => {
    try {
      await companiesApi.deleteCompany(id);
      return id;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to delete company');
      }
      return rejectWithValue('Failed to delete company');
    }
  }
);

const companiesSlice = createSlice({
  name: 'companies',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch companies
      .addCase(fetchCompanies.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchCompanies.fulfilled, (state, action) => {
        state.isLoading = false;
        state.companies = action.payload.data;
        state.totalCount = action.payload.totalCount;
        state.currentPage = action.payload.page;
        state.pageSize = action.payload.pageSize;
      })
      .addCase(fetchCompanies.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch single company
      .addCase(fetchCompany.fulfilled, (state, action) => {
        state.currentCompany = action.payload;
      })
      // Create company
      .addCase(createCompany.fulfilled, (state, action) => {
        state.companies.unshift(action.payload);
        state.totalCount += 1;
      })
      // Update company
      .addCase(updateCompany.fulfilled, (state, action) => {
        const index = state.companies.findIndex(company => company.id === action.payload.id);
        if (index !== -1) {
          state.companies[index] = { ...state.companies[index], ...action.payload.data };
        }
      })
      // Delete company
      .addCase(deleteCompany.fulfilled, (state, action) => {
        state.companies = state.companies.filter(company => company.id !== action.payload);
        state.totalCount -= 1;
      });
  },
});

export const { clearError } = companiesSlice.actions;
export default companiesSlice.reducer;
