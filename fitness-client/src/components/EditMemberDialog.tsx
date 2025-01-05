import { useState } from 'react';
import {
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, Grid, CircularProgress
} from '@mui/material';

export interface EditMemberData {
  memberId: number;
  firstName: string;
  lastName: string;
  email: string;
  address: string;
}

interface EditMemberDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (data: EditMemberData) => Promise<void>;
  initialData: EditMemberData;
  title?: string;
}

export default function EditMemberDialog({ 
  open, 
  onClose, 
  onSave, 
  initialData,
  title = "Edit Member"
}: EditMemberDialogProps) {
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState<EditMemberData>(initialData);

  const handleSubmit = async () => {
    try {
      setLoading(true);
      await onSave(data);
      onClose();
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Grid container spacing={2} sx={{ mt: 1 }}>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="First Name"
              value={data.firstName}
              onChange={(e) => setData(prev => ({ ...prev, firstName: e.target.value }))}
            />
          </Grid>
          <Grid item xs={12} sm={6}>
            <TextField
              fullWidth
              label="Last Name"
              value={data.lastName}
              onChange={(e) => setData(prev => ({ ...prev, lastName: e.target.value }))}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Email"
              type="email"
              value={data.email}
              onChange={(e) => setData(prev => ({ ...prev, email: e.target.value }))}
            />
          </Grid>
          <Grid item xs={12}>
            <TextField
              fullWidth
              label="Address"
              value={data.address}
              onChange={(e) => setData(prev => ({ ...prev, address: e.target.value }))}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button 
          variant="contained" 
          onClick={handleSubmit}
          disabled={loading}
        >
          {loading ? <CircularProgress size={24} /> : 'Save Changes'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
