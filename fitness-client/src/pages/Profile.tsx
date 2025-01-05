import { useState } from 'react';
import { 
  Box, Typography, Paper, Button, Dialog, DialogActions,
  DialogContent, DialogContentText, DialogTitle,
  Grid, CircularProgress, Alert,
} from '@mui/material';
import { useAuth } from '../contexts/AuthContext';
import { memberAPI } from '../services/api';
import { useNavigate } from 'react-router-dom';
import EditMemberDialog, { EditMemberData } from '../components/EditMemberDialog';

export default function Profile() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [isDeleting, setIsDeleting] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const handleEdit = async (data: EditMemberData) => {
    try {
      setError(null);
      await memberAPI.update(user!.memberId, {
        ...data,
        birthday: user!.birthday,
        memberType: user!.memberType
      });
      window.location.reload();
    } catch (err) {
      setError('Failed to update profile');
      console.error('Update error:', err);
      throw err;
    }
  };

  const handleDelete = async () => {
    try {
      setLoading(true);
      setError(null);
      await memberAPI.delete(user!.memberId);
      await logout();
      navigate('/');
    } catch (err) {
      setError('Failed to delete account');
      console.error('Delete error:', err);
    } finally {
      setLoading(false);
      setIsDeleting(false);
    }
  };

  if (!user) return null;

  return (
    <Box sx={{ maxWidth: 600, mx: 'auto', p: 3 }}>
      <Typography variant="h4" gutterBottom>Profile</Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Typography variant="subtitle2">Name</Typography>
            <Typography>{`${user.firstName} ${user.lastName}`}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="subtitle2">Email</Typography>
            <Typography>{user.email}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="subtitle2">Address</Typography>
            <Typography>{user.address}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="subtitle2">Birthday</Typography>
            <Typography>{new Date(user.birthday).toLocaleDateString('en-US', {
              year: 'numeric',
              month: 'long',
              day: 'numeric'
            })}</Typography>
          </Grid>
          <Grid item xs={12}>
            <Typography variant="subtitle2">Membership Type</Typography>
            <Typography sx={{ textTransform: 'capitalize' }}>{user.memberType}</Typography>
          </Grid>
        </Grid>

        <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
          <Button 
            variant="contained" 
            onClick={() => setEditDialogOpen(true)}
          >
            Edit Profile
          </Button>
          <Button 
            variant="outlined" 
            color="error" 
            onClick={() => setIsDeleting(true)}
          >
            Delete Account
          </Button>
        </Box>
      </Paper>

      <EditMemberDialog
        open={editDialogOpen}
        onClose={() => {
          setEditDialogOpen(false);
          setError(null);
        }}
        onSave={handleEdit}
        initialData={{
          memberId: user.memberId,
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          address: user.address
        }}
        title="Edit Profile"
      />

      <Dialog open={isDeleting} onClose={() => setIsDeleting(false)}>
        <DialogTitle>Delete Account</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete your account? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsDeleting(false)}>Cancel</Button>
          <Button 
            onClick={handleDelete} 
            color="error" 
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
