import { useState, useEffect } from 'react';
import { 
  FormControl, InputLabel, Select, MenuItem, 
  SelectChangeEvent, FormHelperText 
} from '@mui/material';
import { TimeSlot, timeSlotAPI } from '../services/api';

interface TimeSlotSelectorProps {
  value: number;
  onChange: (timeSlotId: number) => void;
  date: string;
  equipmentId?: number;
  error?: string;
}

export default function TimeSlotSelector({ 
  value, 
  onChange, 
  date, 
  equipmentId,
  error 
}: TimeSlotSelectorProps) {
  const [timeSlots, setTimeSlots] = useState<TimeSlot[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const fetchTimeSlots = async () => {
      try {
        setLoading(true);
        const response = equipmentId
          ? await timeSlotAPI.getAvailable(date, equipmentId)
          : await timeSlotAPI.getAll();
        setTimeSlots(response.data);
      } catch (error) {
        console.error('Failed to load time slots:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchTimeSlots();
  }, [date, equipmentId]);

  const handleChange = (event: SelectChangeEvent<number>) => {
    onChange(Number(event.target.value));
  };

  return (
    <FormControl fullWidth error={!!error}>
      <InputLabel>Time Slot</InputLabel>
      <Select
        value={value}
        label="Time Slot"
        onChange={handleChange}
        disabled={loading}
      >
        {timeSlots.map((slot) => (
          <MenuItem key={slot.timeSlotId} value={slot.timeSlotId}>
            {`${slot.startTime} - ${slot.endTime}`}
          </MenuItem>
        ))}
      </Select>
      {error && <FormHelperText>{error}</FormHelperText>}
    </FormControl>
  );
}
