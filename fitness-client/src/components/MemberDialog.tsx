import { useState, useEffect } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, MenuItem, Box
} from '@mui/material';
import { Member } from '../services/api';

const MEMBER_TYPES = ['Bronze', 'Silver', 'Gold'];

interface MemberFormData {
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  birthday: string;
  memberType: string;
  interests: string | null;
}

interface MemberDialogProps {
  open: boolean;
  member: Member | null;
  onClose: () => void;
  onSave: (data: MemberFormData) => Promise<void>;
}

export default function MemberDialog({ open, member, onClose, onSave }: MemberDialogProps) {
  const [formData, setFormData] = useState<MemberFormData>({
    firstName: '',
    lastName: '',
    email: '',
    address: '',
    birthday: new Date().toISOString().split('T')[0],
    memberType: 'Bronze',
    interests: null
  });

  useEffect(() => {
    if (member) {
      setFormData({
        firstName: member.firstName,
        lastName: member.lastName,
        email: member.email,
        address: member.address,
        birthday: new Date(member.birthday).toISOString().split('T')[0],
        memberType: member.memberType,
        interests: member.interests
      });
    } else {
      setFormData({
        firstName: '',
        lastName: '',
        email: '',
        address: '',
        birthday: new Date().toISOString().split('T')[0],
        memberType: 'Bronze',
        interests: null
      });
    }
  }, [member]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{member ? 'Edit Member' : 'Add Member'}</DialogTitle>
      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
          <TextField
            label="First Name"
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
            required
          />
          <TextField
            label="Last Name"
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
            required
          />
          <TextField
            label="Email"
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            required
          />
          <TextField
            label="Address"
            value={formData.address}
            onChange={(e) => setFormData({ ...formData, address: e.target.value })}
            required
          />
          <TextField
            label="Birthday"
            type="date"
            value={formData.birthday}
            onChange={(e) => setFormData({ ...formData, birthday: e.target.value })}
            InputLabelProps={{ shrink: true }}
            required
          />
          <TextField
            select
            label="Member Type"
            value={formData.memberType}
            onChange={(e) => setFormData({ ...formData, memberType: e.target.value })}
            required
          >
            {MEMBER_TYPES.map((type) => (
              <MenuItem key={type} value={type}>
                {type}
              </MenuItem>
            ))}
          </TextField>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={() => onSave(formData)} variant="contained">
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}
