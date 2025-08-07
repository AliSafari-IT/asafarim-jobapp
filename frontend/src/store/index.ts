import { configureStore } from '@reduxjs/toolkit';
import authSlice from './slices/authSlice';
import jobApplicationsSlice from './slices/jobApplicationsSlice';
import companiesSlice from './slices/companiesSlice';
import resumesSlice from './slices/resumesSlice';
import feedbackSlice from './slices/feedbackSlice';

export const store = configureStore({
  reducer: {
    auth: authSlice,
    jobApplications: jobApplicationsSlice,
    companies: companiesSlice,
    resumes: resumesSlice,
    feedback: feedbackSlice,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ['persist/PERSIST'],
      },
    }),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
