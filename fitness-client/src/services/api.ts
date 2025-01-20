import axios from 'axios';

const BASE_URL = 'http://localhost:5087/api';

// Add request interceptor for logging
const api = axios.create({
  baseURL: BASE_URL,
});

api.interceptors.request.use(
  (config) => {
    console.log('🌐 API Request:', {
      url: config.url,
      method: config.method,
      data: config.data,
    });
    return config;
  },
  (error) => {
    console.error('❌ API Request Error:', error);
    return Promise.reject(error);
  }
);

api.interceptors.response.use(
  (response) => {
    console.log('✅ API Response:', {
      url: response.config.url,
      status: response.status,
      data: response.data,
    });
    return response;
  },
  (error) => {
    console.error('❌ API Response Error:', {
      url: error.config?.url,
      status: error.response?.status,
      data: error.response?.data,
      message: error.message,
    });
    return Promise.reject(error);
  }
);

// Test API connection on startup
const testConnection = async () => {
  try {
    const response = await api.get('/Test/Connection');
    console.log('🔗 API Connection Test Success:', {
      baseUrl: BASE_URL,
      status: response.status,
      data: response.data
    });
  } catch (error) {
    console.error('🔗 API Connection Test Failed:', {
      baseUrl: BASE_URL,
      error: error instanceof Error ? error.message : 'Unknown error',
      details: axios.isAxiosError(error) ? {
        status: error.response?.status,
        data: error.response?.data,
        message: error.message
      } : null
    });
  }
};

testConnection();

export interface Member {
  memberId: number;
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  birthday: string;
  interests: string | null;
  memberType: string;
}

export interface CreateMemberDto {
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  birthday: string;
  interests: string | null;
  memberType: string;
}

export interface UpdateMemberDto {
  memberId: number;
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  birthday: string;
  memberType: string;
}

export interface UpdateMemberDto {
  memberId: number;
  firstName: string;
  lastName: string;
  email: string;
  address: string;
}

export interface TimeSlot {
  timeSlotId: number;
  startTime: Date | string;  // Update type to handle both Date and string
  endTime: Date | string;    // Update type to handle both Date and string
  slotNumber: number;
  partOfDay: string;
}

export interface Equipment {
  equipmentId: number;
  deviceType: string;  // Changed from description to deviceType
  isAvailable?: boolean;
}

export interface TrainingSession {
  date: string;
  startTime: string;
  duration: number;
  equipmentId: number;
}

export interface CyclingSession extends TrainingSession {
  avgWattage: number;
  maxWattage: number;
  avgCadence: number;
  maxCadence: number;
  trainingType: 'fun' | 'endurance' | 'interval' | 'recovery';
  comments?: string;
}

export interface RunningSession extends TrainingSession {
  avgSpeed: number;
  intervals: RunningInterval[];
}

export interface RunningInterval {
  duration: number;
  speed: number;
}

export interface ReservationDto {
  reservationId: number;
  date: string;
  equipment: string;
  timeSlot: string;
}

export interface MemberReservationView {
  reservationId: number;
  date: string;
  equipment: string;
  timeSlot: string;
}

export interface Reservation {
  reservationId: number;
  equipmentId: number;
  timeSlotId: number;
  date: string;
  memberId: number;
  member: null;
  equipment: null;
  timeSlot: null;
}

export interface CreateReservationDto {
  memberId: number;
  equipmentId: number;
  timeSlotId: number;
  date: string;
  includeNextSlot: boolean;  // Changed from IncludeNextSlot to match C# property
}

export interface DashboardStats {
  totalMembers: number;
  activeReservations: number;
  tableStats: {
    Members: number;
    Reservations: number;
    Equipment: number;
    Programs: number;
  };
}

export interface MemberReservation {
  reservationId: number;
  date: string;
  equipment: string;
  timeSlot: string;
}

export interface UserStats {
  totalReservations: number;
  upcomingReservations: number;
  lastTrainingSession?: {
    date: string;
    type: string;
    duration: number;
  };
}

export interface MonthlyStats {
  year: number;
  month: number;
  totalSessions: number;
  totalHours: number;
  totalDuration: number;
  averageDuration: number;
  sessionsPerType: Record<string, number>;
  sessions: Array<{
    date: string;
    type: string;
    durationMinutes: number;
    details: string;
    trainingImpact: string;
  }>;
}

export interface ActivityOverview {
  totalSessions: number;
  totalDurationHours: number;
  averageDuration: number;
  longestSession: number;
  shortestSession: number;
  sessionsByType: Record<string, number>;
}

export interface YearlySummary {
  [year: number]: {
    [month: number]: number;
  };
}

export const memberAPI = {
  getAll: () => api.get<Member[]>('/members'),
  getById: (id: number) => api.get<Member>(`/members/${id}`),
  create: (data: Omit<Member, 'memberId'>) => api.post<Member>('/members', data),
  update: (id: number, data: UpdateMemberDto) => {
    console.log('Updating member:', { id, data });
    return api.put<Member>(`/members/${id}`, data).catch(error => {
      console.error('Error updating member:', error.response?.data);
      throw new Error(error.response?.data || 'Failed to update member');
    });
  },
  getReservations: (id: number) => api.get<MemberReservationView[]>(`/members/${id}/reservations`),
  delete: (id: number) => api.delete<void>(`/members/${id}`),
};

export const reservationAPI = {
  getAll: () => api.get<Reservation[]>('/reservations'),
  getById: (id: number) => api.get<Reservation>(`/reservations/${id}`),
  create: (data: CreateReservationDto) => {
    console.log('Creating reservation with payload:', data);
    return api.post<ReservationDto[]>('/reservations', data)  // Changed return type to array
      .catch(error => {
        console.error('Reservation creation failed:', error.response?.data);
        throw error;
      });
  },
  delete: (id: number) => api.delete(`/reservations/${id}`),
  getCurrentUserReservations: () => api.get<Reservation[]>('/reservations/me'),
};

export const dashboardAPI = {
  getStats: () => api.get<DashboardStats>('/Test/connection'),
};

export const equipmentAPI = {
  getAll: () => api.get<Equipment[]>('/equipment'),
  getAvailable: (date: string, timeSlotId: number) => 
    api.get<Equipment[]>(`/equipment/available`, {
      params: { date, timeSlotId }
    }),
  setMaintenance: (id: number, inMaintenance: boolean) => 
    api.put(`/equipment/${id}/maintenance`, { inMaintenance }),
};

export const timeSlotAPI = {
  getAll: () => api.get<TimeSlot[]>('/timeslots'),
  getAvailable: (date: string, equipmentId: number) => 
    api.get<TimeSlot[]>(`/timeslots/available`, {
      params: { date, equipmentId }
    }),
};

export const trainingAPI = {
  getCyclingSessions: (memberId: number) => 
    api.get<CyclingSession[]>(`/members/${memberId}/cycling-sessions`),
  getRunningSession: (memberId: number) =>
    api.get<RunningSession[]>(`/members/${memberId}/running-sessions`),
  createCyclingSession: (memberId: number, data: Omit<CyclingSession, 'id'>) =>
    api.post(`/members/${memberId}/cycling-sessions`, data),
  createRunningSession: (memberId: number, data: Omit<RunningSession, 'id'>) =>
    api.post(`/members/${memberId}/running-sessions`, data),
};

export const statisticsAPI = {
  getMonthlyStats: (memberId: number, year: number, month: number) =>
    api.get<MonthlyStats>(`/Statistics/member/${memberId}/monthly`, {
      params: { year, month }
    }),

  getYearlySummary: (memberId: number) => {
    console.log('Fetching yearly summary for member:', memberId);
    return api.get<YearlySummary>(`/Statistics/member/${memberId}/yearly-summary`)
      .then(response => {
        console.log('Yearly summary response:', response.data);
        return response;
      })
      .catch(error => {
        console.error('Error fetching yearly summary:', error);
        throw error;
      });
  },
  
  getOverview: (memberId: number) => {
    const endDate = new Date();
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - 7);
    
    const formatDate = (date: Date) => date.toISOString().split('T')[0];
    
    return api.get<ActivityOverview>(
      `/Statistics/member/${memberId}/overview?startDate=${formatDate(startDate)}&endDate=${formatDate(endDate)}`
    );
  }
};

export default api;
