import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, beforeEach, Mock } from "vitest"; // Removed vi, added Mock for casting window.confirm if needed
import DeleteEvent from "../components/AdminPages/DeleteEvent";
import { mockNavigate, mockGetItem, mockedAxios } from "./testMocks";

// Global mocks from setupTests.ts are used for react-router-dom, axios, localStorage, window.confirm

const mockEventsData = [
  { id: 1, title: "Event 1", startDateTimeUtc: "2025-01-01T10:00:00Z", location: "Location 1", sourceUrl: "http://example.com/1" },
  { id: 2, title: "Event 2", startDateTimeUtc: "2025-02-01T12:00:00Z", location: "Location 2", sourceUrl: "http://example.com/2" },
];

describe("DeleteEvent Component", () => {
  beforeEach(() => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    mockedAxios.get.mockReset();
    mockedAxios.delete.mockReset();
    mockedAxios.get.mockResolvedValue({ data: [...mockEventsData] }); // Provide a fresh copy
    (window.confirm as Mock).mockReset();
    (window.confirm as Mock).mockReturnValue(true); // Default to confirm deletion
  });

  it("renders the component and fetches events", async () => {
    render(<DeleteEvent />);
    expect(screen.getByText("Slet Begivenhed")).toBeInTheDocument();
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith("/api/calendar/events", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
      expect(screen.getByText("-- Vælg en begivenhed --")).toBeInTheDocument();
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
      expect(screen.getByText("Event 2 (ID: 2)")).toBeInTheDocument();
    });
  });

  it("shows an error if not logged in when fetching events", async () => {
    mockGetItem.mockReturnValueOnce(null);
    render(<DeleteEvent />);
    await waitFor(() => {
      expect(screen.getByText("Du er ikke logget ind.")).toBeInTheDocument();
    });
  });

  it("displays event details when an event is selected", async () => {
    render(<DeleteEvent />);
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });

    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), { target: { value: "1" } });

    await waitFor(() => {
      const detailsHeading = screen.getByText("Begivenhedsdetaljer:");
      expect(detailsHeading).toBeInTheDocument();
      const detailsContainer = detailsHeading.parentElement;

      expect(detailsContainer).toHaveTextContent(new RegExp(`Titel:\\s*${mockEventsData[0].title}`));

      const expectedDateString = new Date(mockEventsData[0].startDateTimeUtc).toLocaleString();
      const escapedExpectedDateString = expectedDateString.replace(/[.*+?^${}()|[\\\]\\]/g, "\\$&");
      const dateRegex = new RegExp(`Start:\\s*${escapedExpectedDateString}`);
      expect(detailsContainer).toHaveTextContent(dateRegex);

      expect(detailsContainer).toHaveTextContent(new RegExp(`Lokation:\\s*${mockEventsData[0].location}`));
      expect(detailsContainer).toHaveTextContent(new RegExp(`Kilde URL:\\s*${mockEventsData[0].sourceUrl}`));
    });
  });

  it("prompts for confirmation and deletes event if confirmed", async () => {
    mockedAxios.delete.mockResolvedValue({ data: { message: "Begivenhed slettet succesfuldt!" } });
    render(<DeleteEvent />);
    // Wait for the select to be populated
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), { target: { value: "1" } });

    await waitFor(() => {
      // Ensure details are shown before clicking delete
      expect(screen.getByText("Begivenhedsdetaljer:")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /Slet Begivenhed/i }));

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne begivenhed?");

    await waitFor(() => {
      expect(mockedAxios.delete).toHaveBeenCalledWith("/api/calendar/events/1", {
        headers: { Authorization: "Bearer fake-jwt-token" },
      });
      expect(screen.getByText("Begivenhed slettet succesfuldt!")).toBeInTheDocument();
      // Check if the event is removed from the dropdown
      expect(screen.queryByText("Event 1 (ID: 1)")).not.toBeInTheDocument();
      expect(screen.getByText("Event 2 (ID: 2)")).toBeInTheDocument(); // Other events should remain
    });
  });

  it("does not delete event if confirmation is denied", async () => {
    (window.confirm as Mock).mockReturnValueOnce(false); // Set specific return value for this test
    render(<DeleteEvent />);
    // Wait for the select to be populated
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), { target: { value: "1" } });

    await waitFor(() => {
      // Ensure details are shown
      expect(screen.getByText("Begivenhedsdetaljer:")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /Slet Begivenhed/i }));

    expect(window.confirm).toHaveBeenCalledWith("Er du sikker på, at du vil slette denne begivenhed?");
    expect(mockedAxios.delete).not.toHaveBeenCalled();
  });

  it("shows an error if not logged in when attempting to delete", async () => {
    render(<DeleteEvent />);
    // Wait for the select to be populated
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByText("Begivenhedsdetaljer:")).toBeInTheDocument();
    });

    mockGetItem.mockReturnValueOnce(null); // Simulate logout before clicking delete

    fireEvent.click(screen.getByRole("button", { name: /Slet Begivenhed/i }));

    await waitFor(() => {
      expect(screen.getByText("Du er ikke logget ind.")).toBeInTheDocument();
    });
  });

  it("handles API error on event deletion", async () => {
    mockedAxios.delete.mockRejectedValue({
      isAxiosError: true,
      response: { data: { message: "Failed to delete event" } },
    });
    render(<DeleteEvent />);
    // Wait for the select to be populated
    await waitFor(() => {
      expect(screen.getByText("Event 1 (ID: 1)")).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText("Vælg Begivenhed"), { target: { value: "1" } });

    await waitFor(() => {
      expect(screen.getByText("Begivenhedsdetaljer:")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /Slet Begivenhed/i }));

    await waitFor(() => {
      expect(screen.getByText("En ukendt fejl opstod.")).toBeInTheDocument();
    });
  });

  it("handles API error on fetching events", async () => {
    mockGetItem.mockReturnValue("fake-jwt-token");
    mockedAxios.get.mockReset(); // Reset before setting a new mock value for this specific test
    mockedAxios.get.mockRejectedValueOnce({
      isAxiosError: true,
      response: { data: { message: "Failed to fetch events" } },
    });
    render(<DeleteEvent />);
    await waitFor(() => {
      expect(screen.getByText("Kunne ikke hente begivenheder.")).toBeInTheDocument();
    });
  });

  it("navigates back to admin content page", async () => {
    render(<DeleteEvent />);
    await waitFor(() => {
      // Wait for initial render and potential fetches
      expect(screen.getByRole("button", { name: /Tilbage til Admin Indhold/i })).toBeInTheDocument();
    });
    fireEvent.click(screen.getByRole("button", { name: /Tilbage til Admin Indhold/i }));
    expect(mockNavigate).toHaveBeenCalledWith("/admin/Indhold");
  });
});
