import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { getDashboard } from '../../store/slices/jobApplicationsSlice';
import type { JobApplication } from '../../store/slices/jobApplicationsSlice';
import LoadingSpinner from '../UI/LoadingSpinner';
import { 
  FileText, 
  Building, 
  Clock, 
  TrendingUp,
  Calendar,
  Target
} from 'lucide-react';

// Interface for upcoming interviews
interface Interview {
  id: number;
  jobTitle: string;
  companyName: string;
  interviewDate: string;
}

const Dashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const { dashboard, isLoading } = useAppSelector((state) => state.jobApplications);

  useEffect(() => {
    dispatch(getDashboard());
  }, [dispatch]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <LoadingSpinner size="lg" />
      </div>
    );
  }

  const stats = [
    {
      name: 'Total Applications',
      value: dashboard?.totalApplications || 0,
      icon: FileText,
      color: 'bg-blue-500',
    },
    {
      name: 'Active Applications',
      value: dashboard?.activeApplications || 0,
      icon: Clock,
      color: 'bg-yellow-500',
    },
    {
      name: 'Interviews Scheduled',
      value: dashboard?.interviewsScheduled || 0,
      icon: Calendar,
      color: 'bg-green-500',
    },
    {
      name: 'Offers Received',
      value: dashboard?.offersReceived || 0,
      icon: Target,
      color: 'bg-purple-500',
    },
  ];

  const recentApplications = dashboard?.recentApplications || [];
  const upcomingInterviews = dashboard?.upcomingInterviews || [];

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'applied':
        return 'bg-blue-100 text-blue-800';
      case 'interview':
        return 'bg-yellow-100 text-yellow-800';
      case 'offer':
        return 'bg-green-100 text-green-800';
      case 'rejected':
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">
          Overview of your job application progress
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          return (
            <div
              key={stat.name}
              className="relative bg-white pt-5 px-4 pb-12 sm:pt-6 sm:px-6 shadow rounded-lg overflow-hidden"
            >
              <dt>
                <div className={`absolute ${stat.color} rounded-md p-3`}>
                  <Icon className="h-6 w-6 text-white" />
                </div>
                <p className="ml-16 text-sm font-medium text-gray-500 truncate">
                  {stat.name}
                </p>
              </dt>
              <dd className="ml-16 pb-6 flex items-baseline sm:pb-7">
                <p className="text-2xl font-semibold text-gray-900">
                  {stat.value}
                </p>
              </dd>
            </div>
          );
        })}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Applications */}
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
              Recent Applications
            </h3>
            {recentApplications.length === 0 ? (
              <p className="text-gray-500 text-sm">No recent applications</p>
            ) : (
              <div className="space-y-3">
                {recentApplications.slice(0, 5).map((application: JobApplication) => (
                  <div
                    key={application.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  >
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {application.jobTitle}
                      </p>
                      <p className="text-sm text-gray-500 truncate">
                        {application.companyName}
                      </p>
                    </div>
                    <div className="flex items-center space-x-2">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusColor(
                          application.status
                        )}`}
                      >
                        {application.status}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Upcoming Interviews */}
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
              Upcoming Interviews
            </h3>
            {upcomingInterviews.length === 0 ? (
              <p className="text-gray-500 text-sm">No upcoming interviews</p>
            ) : (
              <div className="space-y-3">
                {upcomingInterviews.slice(0, 5).map((interview: Interview) => (
                  <div
                    key={interview.id}
                    className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                  >
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 truncate">
                        {interview.jobTitle}
                      </p>
                      <p className="text-sm text-gray-500 truncate">
                        {interview.companyName}
                      </p>
                      <p className="text-xs text-gray-400">
                        {new Date(interview.interviewDate).toLocaleDateString()}
                      </p>
                    </div>
                    <div className="flex items-center">
                      <Calendar className="h-4 w-4 text-green-500" />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white shadow rounded-lg">
        <div className="px-4 py-5 sm:p-6">
          <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
            Quick Actions
          </h3>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <button className="btn-primary flex items-center justify-center space-x-2 py-3">
              <FileText className="h-5 w-5" />
              <span>Add Application</span>
            </button>
            <button className="btn-secondary flex items-center justify-center space-x-2 py-3">
              <Building className="h-5 w-5" />
              <span>Add Company</span>
            </button>
            <button className="btn-secondary flex items-center justify-center space-x-2 py-3">
              <TrendingUp className="h-5 w-5" />
              <span>View Reports</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;
