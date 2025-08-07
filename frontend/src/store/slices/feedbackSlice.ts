import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { feedbackApi } from '../../services/api';

export enum FeedbackType {
  General = 'General',
  Interview = 'Interview',
  PhoneScreen = 'PhoneScreen',
  TechnicalInterview = 'TechnicalInterview',
  OnSite = 'OnSite',
  Rejection = 'Rejection',
  Offer = 'Offer',
  FollowUp = 'FollowUp',
  Reference = 'Reference'
}

export interface Feedback {
  id: number;
  jobApplicationId: number;
  jobApplicationTitle?: string;
  type: FeedbackType;
  title: string;
  content: string;
  scheduledFollowUpDate?: string;
  isFollowUpCompleted: boolean;
  interviewerName?: string;
  interviewType?: string;
  rating?: number;
  attachmentPaths: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateFeedbackDto {
  jobApplicationId: number;
  type: FeedbackType;
  title: string;
  content: string;
  scheduledFollowUpDate?: string;
  interviewerName?: string;
  interviewType?: string;
  rating?: number;
  attachmentPaths?: string[];
}

interface FeedbackState {
  feedbacks: Feedback[];
  currentFeedback: Feedback | null;
  pendingFollowUps: Feedback[];
  isLoading: boolean;
  error: string | null;
}

const initialState: FeedbackState = {
  feedbacks: [],
  currentFeedback: null,
  pendingFollowUps: [],
  isLoading: false,
  error: null,
};

// Async thunks
export const fetchFeedbackForApplication = createAsyncThunk(
  'feedback/fetchFeedbackForApplication',
  async (jobApplicationId: number, { rejectWithValue }) => {
    try {
      const response = await feedbackApi.getFeedbackForApplication(jobApplicationId);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch feedback');
      }
      return rejectWithValue('Failed to fetch feedback');
    }
  }
);

export const fetchFeedback = createAsyncThunk(
  'feedback/fetchFeedback',
  async (id: number, { rejectWithValue }) => {
    try {
      const response = await feedbackApi.getFeedback(id);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch feedback');
      }
      return rejectWithValue('Failed to fetch feedback');
    }
  }
);

export const createFeedback = createAsyncThunk(
  'feedback/createFeedback',
  async (data: CreateFeedbackDto, { rejectWithValue }) => {
    try {
      const response = await feedbackApi.createFeedback(data);
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to create feedback');
      }
      return rejectWithValue('Failed to create feedback');
    }
  }
);

export const updateFeedback = createAsyncThunk(
  'feedback/updateFeedback',
  async ({ id, data }: { id: number; data: Partial<CreateFeedbackDto> }, { rejectWithValue }) => {
    try {
      await feedbackApi.updateFeedback(id, data);
      return { id, data };
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to update feedback');
      }
      return rejectWithValue('Failed to update feedback');
    }
  }
);

export const deleteFeedback = createAsyncThunk(
  'feedback/deleteFeedback',
  async (id: number, { rejectWithValue }) => {
    try {
      await feedbackApi.deleteFeedback(id);
      return id;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to delete feedback');
      }
      return rejectWithValue('Failed to delete feedback');
    }
  }
);

export const fetchPendingFollowUps = createAsyncThunk(
  'feedback/fetchPendingFollowUps',
  async (_, { rejectWithValue }) => {
    try {
      const response = await feedbackApi.getPendingFollowUps();
      return response;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to fetch follow-ups');
      }
      return rejectWithValue('Failed to fetch follow-ups');
    }
  }
);

export const completeFollowUp = createAsyncThunk(
  'feedback/completeFollowUp',
  async (id: number, { rejectWithValue }) => {
    try {
      await feedbackApi.completeFollowUp(id);
      return id;
    } catch (error: unknown) {
      if (error && typeof error === 'object' && 'response' in error) {
        const err = error as { response?: { data?: { message?: string } } };
        return rejectWithValue(err.response?.data?.message || 'Failed to complete follow-up');
      }
      return rejectWithValue('Failed to complete follow-up');
    }
  }
);

const feedbackSlice = createSlice({
  name: 'feedback',
  initialState,
  reducers: {
    clearError: (state) => {
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      // Fetch feedback for application
      .addCase(fetchFeedbackForApplication.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchFeedbackForApplication.fulfilled, (state, action) => {
        state.isLoading = false;
        state.feedbacks = action.payload;
      })
      .addCase(fetchFeedbackForApplication.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.payload as string;
      })
      // Fetch single feedback
      .addCase(fetchFeedback.fulfilled, (state, action) => {
        state.currentFeedback = action.payload;
      })
      // Create feedback
      .addCase(createFeedback.fulfilled, (state, action) => {
        state.feedbacks.unshift(action.payload);
      })
      // Update feedback
      .addCase(updateFeedback.fulfilled, (state, action) => {
        const index = state.feedbacks.findIndex(feedback => feedback.id === action.payload.id);
        if (index !== -1) {
          state.feedbacks[index] = { ...state.feedbacks[index], ...action.payload.data };
        }
      })
      // Delete feedback
      .addCase(deleteFeedback.fulfilled, (state, action) => {
        state.feedbacks = state.feedbacks.filter(feedback => feedback.id !== action.payload);
      })
      // Fetch pending follow-ups
      .addCase(fetchPendingFollowUps.fulfilled, (state, action) => {
        state.pendingFollowUps = action.payload;
      })
      // Complete follow-up
      .addCase(completeFollowUp.fulfilled, (state, action) => {
        const index = state.feedbacks.findIndex(feedback => feedback.id === action.payload);
        if (index !== -1) {
          state.feedbacks[index].isFollowUpCompleted = true;
        }
        state.pendingFollowUps = state.pendingFollowUps.filter(feedback => feedback.id !== action.payload);
      });
  },
});

export const { clearError } = feedbackSlice.actions;
export default feedbackSlice.reducer;
