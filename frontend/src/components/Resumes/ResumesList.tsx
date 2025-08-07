import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { fetchResumes } from '../../store/slices/resumesSlice';
import LoadingSpinner from '../UI/LoadingSpinner';
import { FileText, Plus, Search, Download, Star } from 'lucide-react';

const ResumesList: React.FC = () => {
  const dispatch = useAppDispatch();
  const { resumes, isLoading } = useAppSelector((state) => state.resumes);

  useEffect(() => {
    dispatch(fetchResumes({ page: 1, pageSize: 20 }));
  }, [dispatch]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Resumes</h1>
          <p className="mt-1 text-sm text-gray-500">
            Manage your resume versions and AI customizations
          </p>
        </div>
        <button className="btn-primary flex items-center space-x-2">
          <Plus className="h-5 w-5" />
          <span>Upload Resume</span>
        </button>
      </div>

      {/* Search */}
      <div className="bg-white shadow rounded-lg p-6">
        <div className="relative">
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <Search className="h-5 w-5 text-gray-400" />
          </div>
          <input
            type="text"
            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
            placeholder="Search resumes..."
          />
        </div>
      </div>

      {/* Resumes Grid */}
      <div className="bg-white shadow rounded-lg">
        {resumes.length === 0 ? (
          <div className="text-center py-12">
            <FileText className="mx-auto h-12 w-12 text-gray-400" />
            <h3 className="mt-2 text-sm font-medium text-gray-900">No resumes</h3>
            <p className="mt-1 text-sm text-gray-500">
              Get started by uploading your first resume.
            </p>
            <div className="mt-6">
              <button className="btn-primary flex items-center space-x-2 mx-auto">
                <Plus className="h-5 w-5" />
                <span>Upload Resume</span>
              </button>
            </div>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 p-6">
            {resumes.map((resume) => (
              <div key={resume.id} className="border border-gray-200 rounded-lg p-6 hover:shadow-md transition-shadow">
                <div className="flex items-start justify-between">
                  <div className="flex items-center space-x-3">
                    <div className="flex-shrink-0">
                      <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
                        <FileText className="h-6 w-6 text-blue-600" />
                      </div>
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="text-lg font-medium text-gray-900 truncate">
                        {resume.title}
                      </h3>
                      <p className="text-sm text-gray-500">
                        {resume.filePath.split('/').pop() || resume.filePath.split('\\').pop() || resume.filePath}
                      </p>
                    </div>
                  </div>
                  {resume.isDefault && (
                    <Star className="h-5 w-5 text-yellow-400 fill-current" />
                  )}
                </div>
                
                <div className="mt-4">
                  <p className="text-sm text-gray-600 line-clamp-2">
                    {resume.description}
                  </p>
                </div>

                <div className="mt-4">
                  {resume.tags && resume.tags.length > 0 && (
                    <div className="flex flex-wrap gap-1">
                      {resume.tags.slice(0, 3).map((tag, index) => (
                        <span
                          key={index}
                          className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                        >
                          {tag}
                        </span>
                      ))}
                      {resume.tags.length > 3 && (
                        <span className="text-xs text-gray-500">
                          +{resume.tags.length - 3} more
                        </span>
                      )}
                    </div>
                  )}
                </div>

                <div className="mt-6 flex items-center justify-between">
                  <div className="text-sm text-gray-500">
                    1 version
                  </div>
                  <div className="flex items-center space-x-2">
                    <button className="p-2 text-gray-400 hover:text-gray-600">
                      <Download className="h-4 w-4" />
                    </button>
                    <button className="text-primary-600 hover:text-primary-900 text-sm font-medium">
                      View Details
                    </button>
                  </div>
                </div>

                <div className="mt-4 pt-4 border-t border-gray-200">
                  <div className="text-xs text-gray-500">
                    Updated {new Date(resume.updatedAt).toLocaleDateString()}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default ResumesList;
