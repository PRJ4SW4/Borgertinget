import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, expect, beforeEach } from "vitest"; // Removed vi and afterEach
import AddEvent from "../../components/AdminPages/AddEvent";
import { mockNavigate, mockGetItem, mockedAxios } from "../testMocks";

// Global mocks from setupTests.ts are used for react-router-dom, axios, localStorage

describe("AddEvent Component", () => {
  beforeEach(() => {
    // vi.clearAllMocks(); // Handled by global afterEach in setupTests.ts
    mockGetItem.mockReturnValue("fake-jwt-token");
    // Resetting mockedAxios calls for clarity between tests, though clearAllMocks should handle it.
    mockedAxios.post.mockReset();
  });

  it("renders the form correctly", () => {
    render(<AddEvent />);
    expect(screen.getByLabelText(/Titel/i)).toBeInTheDocument();
    expect(
      screen.getByLabelText(/Start Dato\/Tid \(UTC\)/i)
    ).toBeInTheDocument();
    expect(screen.getByLabelText(/Lokation/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Kilde URL/i)).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /Opret Begivenhed/i })
    ).toBeInTheDocument();
  });

  it("shows an error message if not logged in", async () => {
    mockGetItem.mockReturnValueOnce(null); // Simulate not logged in
    render(<AddEvent />);

    fireEvent.submit(screen.getByRole("button", { name: /Opret Begivenhed/i }));

    await waitFor(() => {
      expect(screen.getByText("Du er ikke logget ind.")).toBeInTheDocument();
    });
  });

  it("shows an error message if required fields are missing", async () => {
    render(<AddEvent />);
    fireEvent.submit(screen.getByRole("button", { name: /Opret Begivenhed/i }));

    await waitFor(() => {
      expect(
        screen.getByText(
          "Titel, Start Dato/Tid (UTC), og Kilde URL er påkrævede felter."
        )
      ).toBeInTheDocument();
    });
  });

  it("submits the form data correctly", async () => {
    mockedAxios.post.mockResolvedValue({
      data: { message: "Begivenhed oprettet succesfuldt!" },
    });
    render(<AddEvent />);

    fireEvent.change(screen.getByLabelText(/Titel/i), {
      target: { value: "Test Event" },
    });
    fireEvent.change(screen.getByLabelText(/Start Dato\/Tid \(UTC\)/i), {
      target: { value: "2025-12-31T10:00" },
    });
    fireEvent.change(screen.getByLabelText(/Lokation/i), {
      target: { value: "Test Location" },
    });
    fireEvent.change(screen.getByLabelText(/Kilde URL/i), {
      target: { value: "http://example.com" },
    });

    fireEvent.submit(screen.getByRole("button", { name: /Opret Begivenhed/i }));

    await waitFor(() => {
      expect(mockedAxios.post).toHaveBeenCalledWith(
        "/api/calendar/events",
        {
          title: "Test Event",
          startDateTimeUtc: "2025-12-31T10:00:00Z",
          location: "Test Location",
          sourceUrl: "http://example.com",
        },
        { headers: { Authorization: "Bearer fake-jwt-token" } }
      );
      expect(
        screen.getByText("Begivenhed oprettet succesfuldt!")
      ).toBeInTheDocument();
      expect(mockNavigate).toHaveBeenCalledWith("/admin/Indhold");
    });
  });

  it("handles API error on submit", async () => {
    mockedAxios.post.mockRejectedValue({
      isAxiosError: true,
      response: { data: { message: "Failed to add event" } },
    });
    render(<AddEvent />);

    fireEvent.change(screen.getByLabelText(/Titel/i), {
      target: { value: "Test Event" },
    });
    fireEvent.change(screen.getByLabelText(/Start Dato\/Tid \(UTC\)/i), {
      target: { value: "2025-12-31T10:00" },
    });
    fireEvent.change(screen.getByLabelText(/Kilde URL/i), {
      target: { value: "http://example.com" },
    });

    fireEvent.submit(screen.getByRole("button", { name: /Opret Begivenhed/i }));

    await waitFor(() => {
      expect(screen.getByText("En ukendt fejl opstod.")).toBeInTheDocument();
    });
  });

  it("navigates back to admin content page", () => {
    render(<AddEvent />);
    fireEvent.click(
      screen.getByRole("button", { name: /Tilbage til Admin Indhold/i })
    );
    expect(mockNavigate).toHaveBeenCalledWith("/admin/Indhold");
  });
});
