import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import type { SubmitHandler } from 'react-hook-form';
import { toast } from 'react-hot-toast';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { createJobApplication, ApplicationStatus } from '../../store/slices/jobApplicationsSlice';
import { fetchCompanies } from '../../store/slices/companiesSlice';
import LoadingSpinner from '../UI/LoadingSpinner';

interface JobApplicationFormData {
  jobTitle: string;
  companyId: number;
  location: string;
  applicationDate: string; // Will be mapped to dateApplied
  status: ApplicationStatus;
  notes: string;
}

const JobApplicationForm: React.FC = () => {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { companies, isLoading: isLoadingCompanies } = useAppSelector((state) => state.companies);
  
  useEffect(() => {
    dispatch(fetchCompanies({ page: 1, pageSize: 100 }));
  }, [dispatch]);
  
  const { register, handleSubmit, formState: { errors } } = useForm<JobApplicationFormData>({
    defaultValues: {
      jobTitle: '',
      companyId: 0, // This will need to be selected from a dropdown
      location: '',
      applicationDate: new Date().toISOString().split('T')[0],
      status: ApplicationStatus.Applied,
      notes: ''
    }
  });

  const onSubmit: SubmitHandler<JobApplicationFormData> = async (formData) => {
    try {
      setIsSubmitting(true);
      
      // Map form data to CreateJobApplicationDto
      const applicationData = {
        jobTitle: formData.jobTitle,
        companyId: formData.companyId,
        location: formData.location,
        dateApplied: formData.applicationDate,
        status: formData.status,
        notes: formData.notes
      };
      
      await dispatch(createJobApplication(applicationData)).unwrap();
      toast.success('Job application added successfully!');
      navigate('/applications');
    } catch (error) {
      console.error('Failed to add job application:', error);
      toast.error('Failed to add job application. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto py-8 px-4 sm:px-6 lg:px-8">
      <div className="bg-white shadow overflow-hidden sm:rounded-lg">
        <div className="px-4 py-5 sm:px-6">
          <h2 className="text-lg leading-6 font-medium text-gray-900">Add New Job Application</h2>
          <p className="mt-1 max-w-2xl text-sm text-gray-500">
            Enter the details of your job application
          </p>
        </div>
        <div className="border-t border-gray-200">
          <form onSubmit={handleSubmit(onSubmit)} className="px-4 py-5 sm:p-6">
            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
              <div>
                <label htmlFor="jobTitle" className="block text-sm font-medium text-gray-700">
                  Job Title *
                </label>
                <input
                  type="text"
                  id="jobTitle"
                  {...register('jobTitle', { required: 'Job title is required' })}
                  className={`mt-1 shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md ${
                    errors.jobTitle ? 'border-red-500' : ''
                  }`}
                />
                {errors.jobTitle && (
                  <p className="mt-1 text-sm text-red-600">{errors.jobTitle.message}</p>
                )}
              </div>

              <div>
                <label htmlFor="companyId" className="block text-sm font-medium text-gray-700">
                  Company *
                </label>
                <select
                  id="companyId"
                  {...register('companyId', { 
                    required: 'Company is required',
                    valueAsNumber: true
                  })}
                  className={`mt-1 shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md ${
                    errors.companyId ? 'border-red-500' : ''
                  }`}
                  disabled={isLoadingCompanies}
                >
                  <option value="">Select a company</option>
                  {companies.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
                {errors.companyId && (
                  <p className="mt-1 text-sm text-red-600">{errors.companyId.message}</p>
                )}
                {isLoadingCompanies && <p className="mt-1 text-sm text-gray-500">Loading companies...</p>}
              </div>

              <div className="sm:col-span-3">
                <label htmlFor="location" className="block text-sm font-medium text-gray-700">
                  Location
                </label>
                <div className="mt-1">
                  <input
                    type="text"
                    id="location"
                    {...register('location')}
                    className="shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </div>

              <div className="sm:col-span-3">
                <label htmlFor="applicationDate" className="block text-sm font-medium text-gray-700">
                  Application Date *
                </label>
                <div className="mt-1">
                  <input
                    type="date"
                    id="applicationDate"
                    {...register('applicationDate', { required: 'Application date is required' })}
                    className={`shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md ${
                      errors.applicationDate ? 'border-red-500' : ''
                    }`}
                  />
                </div>
                {errors.applicationDate && (
                  <p className="mt-1 text-sm text-red-600">{errors.applicationDate.message}</p>
                )}
              </div>

              <div className="sm:col-span-3">
                <label htmlFor="status" className="block text-sm font-medium text-gray-700">
                  Status *
                </label>
                <div className="mt-1">
                  <select
                    id="status"
                    {...register('status', { required: 'Status is required' })}
                    className={`shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md ${
                      errors.status ? 'border-red-500' : ''
                    }`}
                  >
                    <option value={ApplicationStatus.Applied}>{ApplicationStatus.Applied}</option>
                    <option value={ApplicationStatus.Interviewing}>{ApplicationStatus.Interviewing}</option>
                    <option value={ApplicationStatus.Offer}>{ApplicationStatus.Offer}</option>
                    <option value={ApplicationStatus.Rejected}>{ApplicationStatus.Rejected}</option>
                    <option value={ApplicationStatus.Withdrawn}>{ApplicationStatus.Withdrawn}</option>
                  </select>
                </div>
                {errors.status && (
                  <p className="mt-1 text-sm text-red-600">{errors.status.message}</p>
                )}
              </div>

              <div className="sm:col-span-6">
                <label htmlFor="notes" className="block text-sm font-medium text-gray-700">
                  Notes
                </label>
                <div className="mt-1">
                  <textarea
                    id="notes"
                    rows={4}
                    {...register('notes')}
                    className="shadow-sm focus:ring-indigo-500 focus:border-indigo-500 block w-full sm:text-sm border-gray-300 rounded-md"
                  />
                </div>
              </div>
            </div>

            <div className="mt-6 flex justify-end space-x-3">
              <button
                type="button"
                onClick={() => navigate('/applications')}
                className="bg-white py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={isSubmitting}
                className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
              >
                {isSubmitting ? <LoadingSpinner size="sm" /> : 'Save'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default JobApplicationForm;
