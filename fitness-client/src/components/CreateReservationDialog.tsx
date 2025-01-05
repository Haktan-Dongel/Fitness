import { useState, useEffect } from 'react';
import { 
  Dialog, DialogTitle, DialogContent, DialogActions,
  Button, TextField, MenuItem, Alert,
  CircularProgress, FormControl, InputLabel, Select, Grid
} from '@mui/material';
import { CreateReservationDto, equipmentAPI, timeSlotAPI, Equipment as ApiEquipment } from '../services/api';

interface Equipment extends ApiEquipment {
  name: string;
  description: string;
}

interface TimeSlot {
  timeSlotId: number;
  startTime: string | Date;
  endTime: string | Date;
}

interface CreateReservationDialogProps {
  open: boolean;
  onClose: () => void;
  onSave: (data: CreateReservationDto) => Promise<void>;
}

export default function CreateReservationDialog({ open, onClose, onSave }: CreateReservationDialogProps) {
  const [formData, setFormData] = useState<CreateReservationDto>({
    date: new Date().toISOString().split('T')[0],
    timeSlotId: 0,
    equipmentId: 0,
    memberId: 1
  });
  const [availableEquipment, setAvailableEquipment] = useState<Equipment[]>([]);
  const [availableTimeSlots, setAvailableTimeSlots] = useState<TimeSlot[]>([]);
  const [loading, setLoading] = useState(false);
  const [error] = useState<string | null>(null);

  useEffect(() => {
    if (open && formData.date) {
      const fetchAvailability = async () => {
        setLoading(true);
        try {
          let equipment = [];
          let timeSlots = [];
          
          if (formData.timeSlotId) {
            const availableEquipment = await equipmentAPI.getAvailable(formData.date, formData.timeSlotId);
            equipment = availableEquipment.data.map(e => ({
              ...e,
              name: e.deviceType,
              description: `Equipment #${e.equipmentId}`
            }));
          } else {
            const allEquipment = await equipmentAPI.getAll();
            equipment = allEquipment.data.map(e => ({
              ...e,
              name: e.deviceType,
              description: `Equipment #${e.equipmentId}`
            }));
          }

          if (formData.equipmentId) {
            const availableTimeSlots = await timeSlotAPI.getAvailable(formData.date, formData.equipmentId);
            timeSlots = availableTimeSlots.data;
          } else {
            const allTimeSlots = await timeSlotAPI.getAll();
            timeSlots = allTimeSlots.data;
          }

          setAvailableEquipment(equipment);
          setAvailableTimeSlots(timeSlots);
        } catch (error) {
          console.error('Error fetching availability:', error);
        }
        setLoading(false);
      };

      fetchAvailability();
    }
  }, [open, formData.date, formData.timeSlotId, formData.equipmentId]);

  const formatTimeSlot = (timeSlot: TimeSlot): string => {
    const start = timeSlot.startTime.toString().split('T')[1]?.substring(0, 5) || timeSlot.startTime;
    const end = timeSlot.endTime.toString().split('T')[1]?.substring(0, 5) || timeSlot.endTime;
    return `${start} - ${end}`;
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Create New Reservation</DialogTitle>
      <DialogContent>
        {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
        <Grid container spacing={2} sx={{ mt: 1 }}>
          <Grid item xs={12}>
            <TextField
              fullWidth
              type="date"
              label="Date"
              value={formData.date}
              onChange={(e) => setFormData({ ...formData, date: e.target.value })}
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Equipment</InputLabel>
              <Select
                value={formData.equipmentId || ''}
                onChange={(e) => setFormData({ ...formData, equipmentId: Number(e.target.value) })}
                disabled={loading}
              >
                {availableEquipment.map((item) => (
                  <MenuItem key={item.equipmentId} value={item.equipmentId}>
                    {`#${item.equipmentId} - ${item.deviceType}`}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={12}>
            <FormControl fullWidth>
              <InputLabel>Time Slot</InputLabel>
              <Select
                value={formData.timeSlotId || ''}
                onChange={(e) => setFormData({ ...formData, timeSlotId: Number(e.target.value) })}
                disabled={loading}
              >
                {availableTimeSlots.map((slot) => (
                  <MenuItem key={slot.timeSlotId} value={slot.timeSlotId}>
                    {formatTimeSlot(slot)}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button 
          onClick={() => onSave(formData)}
          disabled={loading || !formData.date || !formData.equipmentId || !formData.timeSlotId}
        >
          {loading ? <CircularProgress size={24} /> : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
