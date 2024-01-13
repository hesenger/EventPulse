import React, { useState } from "react";
import { format } from "date-fns";
import { DayPicker } from "react-day-picker";
import "react-day-picker/dist/style.css";
import "./custom.css";

export default function App() {
  const [selectedDay, setSelectedDay] = useState();

  const footer = selectedDay ? (
    <p>{format(selectedDay, "PPP")}.</p>
  ) : (
    <p>Please pick a day.</p>
  );

  return (
    <div class="container">
      <div class="row">
        <div class="col">
          <DayPicker
            mode="single"
            selected={selectedDay}
            onSelect={setSelectedDay}
          />
        </div>
        <div class="col">{footer}</div>
      </div>
    </div>
  );
}
