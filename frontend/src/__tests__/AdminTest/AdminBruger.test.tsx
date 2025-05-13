import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, it, beforeEach, expect } from "vitest";
import AdminBruger from "../../components/AdminPages/AdminBruger";
import { BrowserRouter } from "react-router-dom";
import { mockedAxios, mockLocalStorage, mockAlert, createAxiosError } from "../testMocks";

describe("AdminBruger component", () => {
  beforeEach(() => {
    mockLocalStorage();
    mockAlert();
    localStorage.setItem("jwt", "fake-token");

    mockedAxios.get.mockReset();
    mockedAxios.put.mockReset();
  });

  it("shows alert if fields are empty", () => {
    render(
      <BrowserRouter>
        <AdminBruger />
      </BrowserRouter>
    );

    fireEvent.click(screen.getByText("Ændrer brugernavn"));

    expect(window.alert).toHaveBeenCalledWith("Udfyld både det gamle og det nye brugernavn.");
    expect(mockedAxios.get).not.toHaveBeenCalled();
    expect(mockedAxios.put).not.toHaveBeenCalled();
  });

  it("successfully updates username", async () => {
    // Mock GET for user ID and PUT for update
    mockedAxios.get.mockResolvedValueOnce({ data: 123 });
    mockedAxios.put.mockResolvedValueOnce({ status: 200 });

    render(
      <BrowserRouter>
        <AdminBruger />
      </BrowserRouter>
    );

    // Fill in input fields
    fireEvent.change(screen.getByPlaceholderText("Gammel brugernavn"), {
      target: { value: "olduser" },
    });

    fireEvent.change(screen.getByPlaceholderText("Ny brugernavn"), {
      target: { value: "newuser" },
    });

    // Click button
    fireEvent.click(screen.getByText("Ændrer brugernavn"));

    // Wait for axios calls and alert
    await waitFor(() => {
      expect(mockedAxios.get).toHaveBeenCalledWith(
        "/api/administrator/username",
        expect.objectContaining({
          params: { username: "olduser" },
        })
      );

      expect(mockedAxios.put).toHaveBeenCalledWith(
        "/api/administrator/123",
        { userName: "newuser" },
        expect.any(Object)
      );

      expect(window.alert).toHaveBeenCalledWith("Brugernavn opdateret!");
    });

    // Inputs should be cleared
    expect(screen.getByPlaceholderText("Gammel brugernavn")).toHaveValue("");
    expect(screen.getByPlaceholderText("Ny brugernavn")).toHaveValue("");
  });

  it("shows alert if user not found (404)", async () => {
    mockedAxios.get.mockRejectedValueOnce(createAxiosError(404, "Not Found"));

    render(
      <BrowserRouter>
        <AdminBruger />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Gammel brugernavn"), {
      target: { value: "missinguser" },
    });

    fireEvent.change(screen.getByPlaceholderText("Ny brugernavn"), {
      target: { value: "newname" },
    });

    fireEvent.click(screen.getByText("Ændrer brugernavn"));

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Fejl: Bruger ikke fundet");
    });
  });

  it("shows alert for general update error", async () => {
    // Simulate success for GET, failure for PUT
    mockedAxios.get.mockResolvedValueOnce({ data: 456 });
    mockedAxios.put.mockRejectedValueOnce(createAxiosError(500, "Internal Server Error"));


    render(
      <BrowserRouter>
        <AdminBruger />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Gammel brugernavn"), {
      target: { value: "anyuser" },
    });

    fireEvent.change(screen.getByPlaceholderText("Ny brugernavn"), {
      target: { value: "failupdate" },
    });

    fireEvent.click(screen.getByText("Ændrer brugernavn"));

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Fejl under opdatering af brugernavn");
    });
  });

  it("shows alert for unknown error", async () => {
    // Simulate a non-Axios error (e.g. unexpected crash)
    mockedAxios.get.mockRejectedValueOnce("Unexpected crash");

    render(
      <BrowserRouter>
        <AdminBruger />
      </BrowserRouter>
    );

    fireEvent.change(screen.getByPlaceholderText("Gammel brugernavn"), {
      target: { value: "erruser" },
    });

    fireEvent.change(screen.getByPlaceholderText("Ny brugernavn"), {
      target: { value: "test" },
    });

    fireEvent.click(screen.getByText("Ændrer brugernavn"));

    await waitFor(() => {
      expect(window.alert).toHaveBeenCalledWith("Uventet fejl opstod.");
    });
  });
});
