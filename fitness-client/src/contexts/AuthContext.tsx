import { createContext, useContext, useState, ReactNode } from 'react';
import { memberAPI, Member } from '../services/api';

interface AuthContextType {
  user: Member | null;
  login: (email: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isStaff: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<Member | null>(null);

  const login = async (email: string) => {
    try {
      // Get all members en controleer of de gebruiker bestaat adhv email
      const response = await memberAPI.getAll();
      const member = response.data.find(m => m.email.toLowerCase() === email.toLowerCase());
      
      if (!member) {
        throw new Error('Member not found');
      }

      setUser(member);
    } catch (error) {
      console.error('Login failed:', error);
      throw error;
    }
  };

  const logout = () => {
    setUser(null);
  };

  const isStaff = user?.memberType === 'Staff';

  return (
    <AuthContext.Provider value={{ 
      user, 
      login, 
      logout, 
      isAuthenticated: !!user,
      isStaff
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
