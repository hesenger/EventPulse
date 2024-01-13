import React, { useState } from "react";
import { format } from "date-fns";
import { DayPicker } from "react-day-picker";
import "react-day-picker/dist/style.css";
import "./custom.css";

export default function App() {
  const [selectedDay, setSelectedDay] = useState();

  const selection = selectedDay
    ? format(selectedDay, "PPP")
    : "Please pick a day.";

  return (
    <div className="container">
      <header className="d-flex flex-wrap justify-content-center py-3 mb-4 border-bottom">
        <a
          href="/"
          className="d-flex align-items-center mb-3 mb-md-0 me-md-auto link-body-emphasis text-decoration-none"
        >
          <span className="fs-4">Hotel manager</span>
        </a>
      </header>
      <div className="d-flex flex-row">
        <DayPicker
          mode="single"
          selected={selectedDay}
          onSelect={setSelectedDay}
          showOutsideDays
        />
        <div className="flex-fill">
          <h3>{selection}</h3>
        </div>
      </div>
    </div>
  );
}
